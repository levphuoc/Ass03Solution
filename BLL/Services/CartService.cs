using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJSRuntime _jsRuntime;
        private readonly IProductService _productService;
        private readonly ILogger<CartService> _logger;

        public CartService(IJSRuntime jsRuntime, IProductService productService, 
                          IUnitOfWork unitOfWork, ILogger<CartService> logger = null)
        {
            _jsRuntime = jsRuntime;
            _productService = productService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Lấy thông tin giỏ hàng từ DB
        public async Task<CartDTO> GetCartAsync(int memberId)
        {
            try
            {
                var cart = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                
                if (cart == null)
                {
                    return new CartDTO();
                }

                // Create the cart items with explicitly calculated totals
                var cartItems = new List<CartItemDTO>();
                decimal manualTotalCheck = 0;
                
                foreach (var ci in cart.CartItems)
                {
                    var itemDto = new CartItemDTO
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.ProductName,
                        ProductImage = ci.Product.UrlImage ?? "",
                        UnitPrice = ci.UnitPrice,
                        Quantity = ci.Quantity,
                        CategoryName = ci.Product.Category?.CategoryName ?? ""
                    };
                    
                    // Calculate total manually for validation
                    decimal itemTotal = ci.UnitPrice * ci.Quantity;
                    manualTotalCheck += itemTotal;
                    
                    // Log the total calculation for debugging
                    Console.WriteLine($"CART DEBUG: Item {itemDto.ProductName}, Price: {itemDto.UnitPrice}, Qty: {itemDto.Quantity}, Total: {itemTotal}, Calculated Property: {itemDto.Total}");
                    
                    cartItems.Add(itemDto);
                }
                
                var cartDto = new CartDTO
                {
                    Items = cartItems
                };
                
                // Force calculation of totals and verify
                decimal totalAmount = cartDto.TotalAmount;
                Console.WriteLine($"CART DEBUG: Cart manual total check: {manualTotalCheck}");
                Console.WriteLine($"CART DEBUG: Cart property total: {totalAmount}");
                Console.WriteLine($"CART DEBUG: Do they match? {manualTotalCheck == totalAmount}");

                return cartDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Lấy danh sách CartItem từ DB
        public async Task<List<CartItem>> GetCartItemsAsync(int memberId)
        {
            try
            {
                return await _unitOfWork.Carts.GetCartItemsByCartIdAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching cart items for member {memberId}");
                return new List<CartItem>();
            }
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                    quantity = 1;

                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger?.LogWarning($"Cannot add to cart - Product {productId} not found");
                    return new CartDTO { ErrorMessage = "Product not found." };
                }
                
                // Check if product is in stock
                if (product.UnitsInStock <= 0)
                {
                    _logger?.LogWarning($"Cannot add to cart - Product {productId} is out of stock");
                    return new CartDTO { ErrorMessage = $"Product '{product.ProductName}' is out of stock." };
                }
                
                // Check if requested quantity is available
                if (product.UnitsInStock < quantity)
                {
                    _logger?.LogWarning($"Cannot add requested quantity - Product {productId} has only {product.UnitsInStock} units in stock");
                    return new CartDTO { 
                        ErrorMessage = $"Only {product.UnitsInStock} units of '{product.ProductName}' are available." 
                    };
                }

                await _unitOfWork.Carts.AddItemToCartAsync(
                    memberId, 
                    product.ProductId, 
                    quantity, 
                    product.UnitPrice);
                
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error adding product {productId} to cart for member {memberId}");
                return new CartDTO { ErrorMessage = "An error occurred while adding the product to cart." };
            }
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"SERVICE DEBUG: Starting cart item update - MemberId: {memberId}, ProductId: {productId}, Quantity: {quantity}");
                
                // If quantity is 0 or negative, remove the item instead
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(memberId, productId);
                }
                
                // Check product stock availability
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger?.LogWarning($"Cannot update cart - Product {productId} not found");
                    return new CartDTO { ErrorMessage = "Product not found." };
                }
                
                // Check if product is in stock
                if (product.UnitsInStock <= 0)
                {
                    _logger?.LogWarning($"Cannot update cart - Product {productId} is out of stock");
                    // Remove the item from cart since it's out of stock
                    await RemoveFromCartAsync(memberId, productId);
                    return new CartDTO { ErrorMessage = $"⚠️ {product.ProductName} has been removed from your cart because it is OUT OF STOCK." };
                }
                
                // Check if requested quantity is available
                if (product.UnitsInStock < quantity)
                {
                    _logger?.LogWarning($"Cannot update to requested quantity - Product {productId} has only {product.UnitsInStock} units available");
                    // Update to maximum available quantity instead
                    await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, product.UnitsInStock);
                    var cart = await GetCartAsync(memberId);
                    
                    // Make the error message more prominent
                    cart.ErrorMessage = $"⚠️ STOCK LIMITATION: Only {product.UnitsInStock} units of '{product.ProductName}' are available. Your cart has been updated to the maximum quantity.";
                    
                    // Make sure the item in the cart shows the correct quantity
                    var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                    if (cartItem != null && cartItem.Quantity != product.UnitsInStock)
                    {
                        // Force the correct quantity in the DTO
                        cartItem.Quantity = product.UnitsInStock;
                    }
                    
                    return cart;
                }

                // Try the standard repository update
                bool updateResult = await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, quantity);
                
                if (!updateResult)
                {
                    Console.WriteLine($"SERVICE DEBUG: Standard update failed, trying direct update");
                    await ForceUpdateCartItemQuantityAsync(memberId, productId, quantity);
                }
                
                // Get the updated cart after changes
                var updatedCart = await GetCartAsync(memberId);
                Console.WriteLine($"SERVICE DEBUG: Cart update completed. Cart has {updatedCart.Items.Count} items.");
                
                return updatedCart;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating product {productId} quantity in cart for member {memberId}");
                return new CartDTO { ErrorMessage = "An error occurred while updating the cart." };
            }
        }
        
        // Directly update cart item using SQL
        private async Task ForceUpdateCartItemQuantityAsync(int memberId, int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"SERVICE DEBUG: Executing direct SQL update - MemberId: {memberId}, ProductId: {productId}, New Quantity: {quantity}");
                
                // Get a database connection
                var connection = _unitOfWork.GetDbConnection();
                
                // First, find the cart ID directly from the database
                int cartId = 0;
                using (var cmd1 = connection.CreateCommand())
                {
                    cmd1.CommandText = "SELECT CartId FROM Carts WHERE MemberId = @MemberId";
                    var param1 = cmd1.CreateParameter();
                    param1.ParameterName = "@MemberId";
                    param1.Value = memberId;
                    cmd1.Parameters.Add(param1);
                    
                    var result = await cmd1.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        cartId = Convert.ToInt32(result);
                    }
                }
                
                if (cartId == 0)
                {
                    throw new Exception($"No cart found for member {memberId}");
                }
                
                // Get the current unit price directly from the database
                decimal unitPrice = 0;
                using (var cmd2 = connection.CreateCommand())
                {
                    cmd2.CommandText = "SELECT UnitPrice FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
                    var param1 = cmd2.CreateParameter();
                    param1.ParameterName = "@CartId";
                    param1.Value = cartId;
                    cmd2.Parameters.Add(param1);
                    
                    var param2 = cmd2.CreateParameter();
                    param2.ParameterName = "@ProductId";
                    param2.Value = productId;
                    cmd2.Parameters.Add(param2);
                    
                    var result = await cmd2.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        unitPrice = Convert.ToDecimal(result);
                    }
                }
                
                if (unitPrice == 0)
                {
                    throw new Exception($"Cart item for product {productId} not found or has invalid price");
                }
                
                // Calculate the new total price
                decimal totalPrice = unitPrice * quantity;
                Console.WriteLine($"SERVICE DEBUG: Direct SQL update - CartId: {cartId}, ProductId: {productId}, " +
                    $"UnitPrice: {unitPrice}, New Quantity: {quantity}, New TotalPrice: {totalPrice}");
                
                // Update the cart item with new quantity and total price
                using (var cmd3 = connection.CreateCommand())
                {
                    cmd3.CommandText = @"
                        UPDATE CartItems 
                        SET Quantity = @Quantity, 
                            TotalPrice = @TotalPrice,
                            UpdatedAt = @UpdatedAt
                        WHERE CartId = @CartId AND ProductId = @ProductId";
                    
                    var paramQty = cmd3.CreateParameter();
                    paramQty.ParameterName = "@Quantity";
                    paramQty.Value = quantity;
                    cmd3.Parameters.Add(paramQty);
                    
                    var paramTotal = cmd3.CreateParameter();
                    paramTotal.ParameterName = "@TotalPrice";
                    paramTotal.Value = totalPrice;
                    cmd3.Parameters.Add(paramTotal);
                    
                    var paramUpdated = cmd3.CreateParameter();
                    paramUpdated.ParameterName = "@UpdatedAt";
                    paramUpdated.Value = DateTime.UtcNow;
                    cmd3.Parameters.Add(paramUpdated);
                    
                    var paramCartId = cmd3.CreateParameter();
                    paramCartId.ParameterName = "@CartId";
                    paramCartId.Value = cartId;
                    cmd3.Parameters.Add(paramCartId);
                    
                    var paramProductId = cmd3.CreateParameter();
                    paramProductId.ParameterName = "@ProductId";
                    paramProductId.Value = productId;
                    cmd3.Parameters.Add(paramProductId);
                    
                    int rowsAffected = await cmd3.ExecuteNonQueryAsync();
                    Console.WriteLine($"SERVICE DEBUG: Direct SQL update completed. Rows affected: {rowsAffected}");
                    
                    if (rowsAffected == 0)
                    {
                        throw new Exception("SQL Update failed - no rows were updated");
                    }
                }
                
                // Update cart's UpdatedAt timestamp
                using (var cmd4 = connection.CreateCommand())
                {
                    cmd4.CommandText = "UPDATE Carts SET UpdatedAt = @UpdatedAt WHERE CartId = @CartId";
                    
                    var paramUpdated = cmd4.CreateParameter();
                    paramUpdated.ParameterName = "@UpdatedAt";
                    paramUpdated.Value = DateTime.UtcNow;
                    cmd4.Parameters.Add(paramUpdated);
                    
                    var paramCartId = cmd4.CreateParameter();
                    paramCartId.ParameterName = "@CartId";
                    paramCartId.Value = cartId;
                    cmd4.Parameters.Add(paramCartId);
                    
                    await cmd4.ExecuteNonQueryAsync();
                }
                
                // Verify the update using a separate SQL query
                using (var cmd5 = connection.CreateCommand())
                {
                    cmd5.CommandText = "SELECT Quantity, TotalPrice FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
                    
                    var paramCartId = cmd5.CreateParameter();
                    paramCartId.ParameterName = "@CartId";
                    paramCartId.Value = cartId;
                    cmd5.Parameters.Add(paramCartId);
                    
                    var paramProductId = cmd5.CreateParameter();
                    paramProductId.ParameterName = "@ProductId";
                    paramProductId.Value = productId;
                    cmd5.Parameters.Add(paramProductId);
                    
                    using (var reader = await cmd5.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int updatedQty = reader.GetInt32(0);
                            decimal updatedTotal = reader.GetDecimal(1);
                            
                            Console.WriteLine($"SERVICE DEBUG: Verification - Updated values in database: Quantity={updatedQty}, TotalPrice={updatedTotal}");
                            
                            if (updatedQty != quantity)
                            {
                                Console.WriteLine($"WARNING: Database verification shows quantity mismatch: Expected {quantity}, got {updatedQty}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("WARNING: Could not verify update - no matching row found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SERVICE DEBUG: Error in ForceUpdateCartItemQuantityAsync - {ex.Message}");
                
                // One last desperate attempt with raw SQL in a transaction
                try
                {
                    var dbContext = _unitOfWork.GetDbContext();
                    
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Find the cart ID
                            var cartIdResult = await dbContext.Database.ExecuteSqlRawAsync(
                                "SELECT CAST(CartId AS INT) FROM Carts WHERE MemberId = {0}", memberId);
                            
                            // Direct update with transaction
                            await dbContext.Database.ExecuteSqlRawAsync(
                                "UPDATE ci SET ci.Quantity = {0}, ci.TotalPrice = ci.UnitPrice * {0}, ci.UpdatedAt = GETUTCDATE() " +
                                "FROM CartItems ci " +
                                "INNER JOIN Carts c ON ci.CartId = c.CartId " +
                                "WHERE c.MemberId = {1} AND ci.ProductId = {2}",
                                quantity, memberId, productId);
                                
                            await transaction.CommitAsync();
                            
                            Console.WriteLine("SERVICE DEBUG: Emergency transaction commit succeeded");
                        }
                        catch (Exception innerEx)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"SERVICE DEBUG: Emergency transaction failed - {innerEx.Message}");
                            throw;
                        }
                    }
                }
                catch
                {
                    // Just swallow this - we've tried our best
                    Console.WriteLine("SERVICE DEBUG: All SQL update attempts failed");
                    throw; // Rethrow the original exception
                }
            }
        }

        // Xóa một sản phẩm khỏi giỏ hàng
        public async Task<CartDTO> RemoveFromCartAsync(int memberId, int productId)
        {
            try
            {
                _logger?.LogInformation($"Removing product {productId} from cart for member {memberId}");
                
                // First check if this product is the last item in the cart
                var cartBeforeRemoval = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                bool isLastItem = cartBeforeRemoval?.CartItems?.Count <= 1 && 
                                 cartBeforeRemoval.CartItems.Any(ci => ci.ProductId == productId);
                
                if (isLastItem)
                {
                    _logger?.LogInformation($"Product {productId} is the last item in cart {cartBeforeRemoval.CartId}. Cart will be deleted.");
                }
                
                // Remove the item from cart
                bool success = await _unitOfWork.Carts.RemoveItemFromCartAsync(memberId, productId);
                
                if (success)
                {
                    _logger?.LogInformation($"Successfully removed product {productId} from cart for member {memberId}");
                    
                    // Check if cart still exists after removal
                    var cartAfterRemoval = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                    if (cartAfterRemoval == null && isLastItem)
                    {
                        _logger?.LogInformation($"Cart was successfully removed as it had no more items");
                    }
                    else if (isLastItem && cartAfterRemoval != null)
                    {
                        // If this was the last item but cart still exists, force delete it
                        _logger?.LogWarning($"Cart {cartBeforeRemoval.CartId} still exists after removing last item. Force deleting...");
                        await ForceDeleteCartAsync(memberId);
                    }
                }
                
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error removing product {productId} from cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Xóa toàn bộ giỏ hàng
        public async Task<bool> ClearCartAsync(int memberId)
        {
            try
            {
                _logger?.LogInformation($"Clearing entire cart for member {memberId}");
                
                // Get cart before clearing to log details
                var cartBeforeClear = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartBeforeClear != null)
                {
                    _logger?.LogInformation($"Cart {cartBeforeClear.CartId} with {cartBeforeClear.CartItems?.Count ?? 0} items will be deleted");
                }
                
                bool success = await _unitOfWork.Carts.ClearCartAsync(memberId);
                
                if (success)
                {
                    _logger?.LogInformation($"Successfully cleared and removed cart for member {memberId}");
                    
                    // Verify cart was deleted
                    var cartAfterClear = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                    if (cartAfterClear == null)
                    {
                        _logger?.LogInformation($"Verified cart for member {memberId} no longer exists");
                    }
                    else
                    {
                        _logger?.LogWarning($"Cart for member {memberId} still exists after clear operation - this should not happen");
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error clearing cart for member {memberId}");
                return false;
            }
        }

        // Add a private method for direct SQL deletion
        private async Task ForceDeleteCartWithSQLAsync(int memberId, int cartId)
        {
            try
            {
                _logger?.LogWarning($"Using direct SQL to force delete cart {cartId} for member {memberId}");
                
                // Get a database connection from the UnitOfWork
                var connection = _unitOfWork.GetDbConnection();
                
                // Delete cart items first
                using (var command1 = connection.CreateCommand())
                {
                    command1.CommandText = $"DELETE FROM CartItems WHERE CartId = {cartId}";
                    int itemsDeleted = await command1.ExecuteNonQueryAsync();
                    _logger?.LogInformation($"Direct SQL: Deleted {itemsDeleted} items for cart {cartId}");
                }
                
                // Then delete the cart itself
                using (var command2 = connection.CreateCommand())
                {
                    command2.CommandText = $"DELETE FROM Carts WHERE CartId = {cartId}";
                    int cartsDeleted = await command2.ExecuteNonQueryAsync();
                    _logger?.LogInformation($"Direct SQL: Deleted {cartsDeleted} carts with ID {cartId}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in direct SQL deletion: {ex.Message}");
                // Try the absolute last resort approach - use raw SQL through Entity Framework
                try
                {
                    // Get access to the context through reflection if necessary
                    // This is just for debugging purposes to ensure we can delete the cart
                    var dbContext = _unitOfWork.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_unitOfWork) as DbContext;
                    
                    if (dbContext != null)
                    {
                        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM CartItems WHERE CartId = {cartId}");
                        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM Carts WHERE CartId = {cartId}");
                        _logger?.LogInformation($"Last resort SQL through EF Core: Attempted to delete cart {cartId}");
                    }
                }
                catch (Exception innerEx)
                {
                    _logger?.LogError(innerEx, $"Last resort EF Core deletion failed: {innerEx.Message}");
                }
            }
        }

        // Xóa giỏ hàng khi đã tạo đơn hàng thành công
        public async Task DeleteCartAfterOrderCreateAsync(int memberId)
        {
            try
            {
                // First, check if cart exists
                var cart = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cart == null)
                {
                    _logger?.LogInformation($"No cart found for member {memberId} to delete after order creation");
                    return;
                }

                // Log details for debugging
                int cartId = cart.CartId;
                int itemCount = cart.CartItems?.Count ?? 0;
                _logger?.LogInformation($"Starting to delete cart ID {cartId} with {itemCount} items for member {memberId}");
                
                // Delete both cart and cart items completely
                await _unitOfWork.Carts.DeleteCartAndItemsByMemberIdAsync(memberId);
                
                // Verify deletion
                var cartAfterDeletion = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartAfterDeletion != null)
                {
                    _logger?.LogWarning($"Failed to delete cart for member {memberId} - cart still exists after deletion attempt");
                    
                    // Try a second time with a different approach - manual deletion of cart items first, then cart
                    try 
                    {
                        // First clear all cart items
                        await _unitOfWork.Carts.ClearCartAsync(memberId);
                        
                        // Then try to find and delete the cart directly through the generic repository
                        var cartsToDelete = await _unitOfWork.Carts.FindAsync(c => c.MemberId == memberId);
                        foreach (var cartToDelete in cartsToDelete)
                        {
                            await _unitOfWork.Carts.DeleteAsync(cartToDelete.CartId);
                        }
                        
                        // Save changes to ensure everything is committed
                        await _unitOfWork.SaveChangesAsync();
                        
                        _logger?.LogInformation($"Second attempt: Manually deleted cart for member {memberId}");
                        
                        // Verify again
                        cartAfterDeletion = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                        if (cartAfterDeletion != null)
                        {
                            // Third attempt - direct SQL as a last resort
                            await ForceDeleteCartWithSQLAsync(memberId, cartId);
                        }
                    }
                    catch (Exception innerEx) 
                    {
                        _logger?.LogError(innerEx, $"Second attempt: Error manually deleting cart for member {memberId}");
                        // Try direct SQL as a last resort
                        await ForceDeleteCartWithSQLAsync(memberId, cartId);
                    }
                }
                else
                {
                    _logger?.LogInformation($"Cart and items successfully deleted for member {memberId} after order creation");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting cart for member {memberId} after order creation: {ex.Message}");
            }
        }

        // Xóa cart bất kể còn item hay không
        public async Task<bool> ForceDeleteCartAsync(int memberId)
        {
            try
            {
                _logger?.LogInformation($"Force deleting cart for member {memberId}");
                
                // Get cart details before deletion
                var cart = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cart == null)
                {
                    _logger?.LogInformation($"No cart found for member {memberId} to delete");
                    return true; // Consider it a success if cart doesn't exist
                }
                
                int cartId = cart.CartId;
                
                // Try multiple deletion approaches just like in checkout
                
                // First approach: Use repository's DeleteCartAndItemsByMemberIdAsync
                await _unitOfWork.Carts.DeleteCartAndItemsByMemberIdAsync(memberId);
                
                // Verify deletion
                var cartAfterFirstAttempt = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartAfterFirstAttempt == null)
                {
                    _logger?.LogInformation($"Cart {cartId} successfully deleted on first attempt");
                    return true;
                }
                
                // Second approach: Clear cart items then delete cart
                _logger?.LogWarning($"First deletion attempt failed for cart {cartId}, trying second approach");
                
                await _unitOfWork.Carts.ClearCartAsync(memberId);
                
                // Verify second attempt
                var cartAfterSecondAttempt = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartAfterSecondAttempt == null)
                {
                    _logger?.LogInformation($"Cart {cartId} successfully deleted on second attempt");
                    return true;
                }
                
                // Third approach: Direct SQL deletion
                _logger?.LogWarning($"Second deletion attempt failed for cart {cartId}, trying direct SQL approach");
                await ForceDeleteCartWithSQLAsync(memberId, cartId);
                
                // Final verification
                var cartAfterAllAttempts = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartAfterAllAttempts == null)
                {
                    _logger?.LogInformation($"Cart {cartId} successfully deleted after all attempts");
                    return true;
                }
                else
                {
                    _logger?.LogError($"All attempts to delete cart {cartId} failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in ForceDeleteCartAsync for member {memberId}");
                return false;
            }
        }
    }
} 
