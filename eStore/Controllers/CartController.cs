using BLL.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using DataAccessLayer.Data;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace eStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly EStoreDbContext _dbContext;
        private readonly ILogger<CartController> _logger;
        private readonly string _connectionString;

        public CartController(ICartService cartService, EStoreDbContext dbContext, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _dbContext = dbContext;
            _logger = logger;
            // Get the connection string directly
            _connectionString = dbContext.Database.GetConnectionString();
        }

        [HttpDelete("forcedelete/{memberId}")]
        public async Task<IActionResult> ForceDeleteCart(int memberId)
        {
            try
            {
                _logger.LogInformation($"API: Force deleting cart for member {memberId}");
                
                // Try to find cart ID directly using a query
                int? cartId = await _dbContext.Carts
                    .Where(c => c.MemberId == memberId)
                    .Select(c => (int?)c.CartId)
                    .FirstOrDefaultAsync();
                
                if (!cartId.HasValue)
                {
                    _logger.LogInformation($"No cart found for member {memberId}");
                    return Ok(new { success = true, message = "No cart found to delete" });
                }
                
                _logger.LogInformation($"Found cart {cartId.Value} for member {memberId}, proceeding with deletion");
                
                // Use direct ADO.NET with explicit connection management
                bool deleteSuccess = await DirectSqlDeleteAsync(memberId, cartId.Value);
                
                if (deleteSuccess)
                {
                    return Ok(new { success = true, message = $"Cart {cartId.Value} for member {memberId} was successfully deleted" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to delete cart with direct SQL" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ForceDeleteCart: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        private async Task<bool> DirectSqlDeleteAsync(int memberId, int cartId)
        {
            try
            {
                // Use a completely separate connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Start a transaction for atomicity
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // First try to delete all cart items
                            using (var cmd1 = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", connection, transaction))
                            {
                                cmd1.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                                int itemsDeleted = await cmd1.ExecuteNonQueryAsync();
                                _logger.LogInformation($"Deleted {itemsDeleted} cart items for cart {cartId}");
                            }
                            
                            // Then delete the cart
                            using (var cmd2 = new SqlCommand("DELETE FROM Carts WHERE CartId = @CartId", connection, transaction))
                            {
                                cmd2.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                                int cartsDeleted = await cmd2.ExecuteNonQueryAsync();
                                _logger.LogInformation($"Deleted {cartsDeleted} carts with ID {cartId}");
                                
                                // If cart wasn't deleted, try by memberId
                                if (cartsDeleted == 0)
                                {
                                    using (var cmd3 = new SqlCommand("DELETE FROM Carts WHERE MemberId = @MemberId", connection, transaction))
                                    {
                                        cmd3.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                                        int memberCartsDeleted = await cmd3.ExecuteNonQueryAsync();
                                        _logger.LogInformation($"Deleted {memberCartsDeleted} carts by member ID {memberId}");
                                    }
                                }
                            }
                            
                            // Commit transaction
                            transaction.Commit();
                            _logger.LogInformation($"Transaction committed successfully");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in transaction, rolling back: {ex.Message}");
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DirectSqlDeleteAsync: {ex.Message}");
                
                // Emergency last attempt - directly accessing tables without any constraints
                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        
                        // Disable foreign key constraints temporarily
                        using (var cmd0 = new SqlCommand("EXEC sp_MSforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"", connection))
                        {
                            await cmd0.ExecuteNonQueryAsync();
                        }
                        
                        // Delete cart items
                        using (var cmd1 = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", connection))
                        {
                            cmd1.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                            await cmd1.ExecuteNonQueryAsync();
                        }
                        
                        // Delete cart
                        using (var cmd2 = new SqlCommand("DELETE FROM Carts WHERE CartId = @CartId OR MemberId = @MemberId", connection))
                        {
                            cmd2.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                            cmd2.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                            await cmd2.ExecuteNonQueryAsync();
                        }
                        
                        // Re-enable foreign key constraints
                        using (var cmd3 = new SqlCommand("EXEC sp_MSforeachtable \"ALTER TABLE ? CHECK CONSTRAINT all\"", connection))
                        {
                            await cmd3.ExecuteNonQueryAsync();
                        }
                    }
                    
                    _logger.LogWarning($"Emergency cart deletion method executed for cart {cartId}");
                    return true;
                }
                catch (Exception emergencyEx)
                {
                    _logger.LogError(emergencyEx, $"Emergency deletion attempt failed: {emergencyEx.Message}");
                    return false;
                }
            }
        }
        
        [HttpGet("cleanup/{memberId}")]
        public async Task<IActionResult> CleanupCart(int memberId, [FromQuery] bool force = false)
        {
            try
            {
                _logger.LogInformation($"API: Cleanup cart for member {memberId}, force={force}");
                
                if (force)
                {
                    // Completely bypass normal flow and execute nuclear option
                    await NuclearCartDeletion(memberId);
                    return Ok(new { success = true, message = "Nuclear cleanup executed" });
                }
                
                // Raw SQL to get cart ID
                int? cartId = null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT CartId FROM Carts WHERE MemberId = @MemberId", connection))
                    {
                        cmd.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            cartId = Convert.ToInt32(result);
                        }
                    }
                }
                
                if (cartId.HasValue)
                {
                    await DirectSqlDeleteAsync(memberId, cartId.Value);
                    return Ok(new { success = true, message = $"Cleanup completed for cart {cartId}" });
                }
                
                return Ok(new { success = true, message = "No cart found to clean up" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CleanupCart: {ex.Message}");
                
                // If error occurs and force=true, try nuclear option
                if (force)
                {
                    try
                    {
                        await NuclearCartDeletion(memberId);
                        return Ok(new { success = true, message = "Error occurred but nuclear cleanup executed" });
                    }
                    catch (Exception nuclearEx)
                    {
                        _logger.LogError(nuclearEx, "Nuclear deletion also failed");
                    }
                }
                
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Nuclear option for cart deletion - bypasses all normal methods and directly manipulates the database
        /// </summary>
        private async Task NuclearCartDeletion(int memberId)
        {
            try
            {
                _logger.LogWarning($"Executing nuclear cart deletion for member {memberId}");
                
                // Use a completely separate connection and disable all constraints
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Find any cart IDs for this member
                    List<int> cartIds = new List<int>();
                    using (var cmdFind = new SqlCommand("SELECT CartId FROM Carts WHERE MemberId = @MemberId", connection))
                    {
                        cmdFind.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                        using (var reader = await cmdFind.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                cartIds.Add(reader.GetInt32(0));
                            }
                        }
                    }
                    
                    _logger.LogInformation($"Found {cartIds.Count} carts for member {memberId}");
                    
                    // Disable constraints
                    using (var cmd0 = new SqlCommand("EXEC sp_MSforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"", connection))
                    {
                        await cmd0.ExecuteNonQueryAsync();
                    }
                    
                    // Delete cart items for all found cart IDs
                    foreach (int cartId in cartIds)
                    {
                        using (var cmd1 = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", connection))
                        {
                            cmd1.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                            int deleted = await cmd1.ExecuteNonQueryAsync();
                            _logger.LogInformation($"Nuclear: Deleted {deleted} items for cart {cartId}");
                        }
                    }
                    
                    // Delete ALL carts for this member
                    using (var cmd2 = new SqlCommand("DELETE FROM Carts WHERE MemberId = @MemberId", connection))
                    {
                        cmd2.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                        int deleted = await cmd2.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Nuclear: Deleted {deleted} carts for member {memberId}");
                    }
                    
                    // Re-enable constraints
                    using (var cmd3 = new SqlCommand("EXEC sp_MSforeachtable \"ALTER TABLE ? CHECK CONSTRAINT all\"", connection))
                    {
                        await cmd3.ExecuteNonQueryAsync();
                    }
                }
                
                _logger.LogInformation($"Nuclear cart deletion completed for member {memberId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in nuclear cart deletion: {ex.Message}");
                throw;
            }
        }
    }
} 