using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class CartRepository: GenericRepository<Cart>, ICartRepository
    {
       

        public CartRepository(EStoreDbContext context) : base(context) { }

        public async Task<List<CartItem>> GetCartItemsByCartIdAsync(int userId)
        {
            try 
            {
                // Execute everything in a single query to avoid context disposal issues
                return await _context.Carts
                    .Where(c => c.MemberId == userId)
                    .SelectMany(c => _context.CartItems
                        .Where(ci => ci.CartId == c.CartId)
                        .Include(ci => ci.Product))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetCartItemsByCartIdAsync: {ex.Message}");
                return new List<CartItem>();
            }
        }
       
        public async Task DeleteCartAndItemsByMemberIdAsync(int memberId)
        {
            try
            {
                // Use a transaction to ensure atomic deletion
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Find the cart
                    var cart = await _context.Carts
                        .FirstOrDefaultAsync(c => c.MemberId == memberId);

                    if (cart == null)
                    {
                        Console.WriteLine($"No cart found for member {memberId} to delete");
                        await transaction.CommitAsync();
                        return;
                    }

                    int cartId = cart.CartId;
                    Console.WriteLine($"Deleting cart {cartId} for member {memberId}");

                    // Use direct SQL to delete cart items first
                    string deleteCartItemsSQL = $"DELETE FROM CartItems WHERE CartId = {cartId}";
                    await _context.Database.ExecuteSqlRawAsync(deleteCartItemsSQL);
                    Console.WriteLine($"SQL executed: {deleteCartItemsSQL}");
                    
                    // Use direct SQL to delete the cart
                    string deleteCartSQL = $"DELETE FROM Carts WHERE CartId = {cartId}";
                    await _context.Database.ExecuteSqlRawAsync(deleteCartSQL);
                    Console.WriteLine($"SQL executed: {deleteCartSQL}");
                    
                    // Commit the transaction
                    await transaction.CommitAsync();
                    Console.WriteLine($"Successfully deleted cart {cartId} and its items for member {memberId}");
                }
                catch (Exception ex)
                {
                    // Rollback on error
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error in transaction during cart deletion: {ex.Message}");
                    throw; // Rethrow to be handled by caller
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting cart for member {memberId}: {ex.Message}");
                throw; // Rethrow to be handled by caller
            }
        }


        public async Task<Cart> GetCartWithItemsByMemberIdAsync(int memberId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            return cart;
        }

        public async Task<bool> ClearCartAsync(int memberId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
                return false;

            // Remove all cart items
            _context.CartItems.RemoveRange(cart.CartItems);
            
            // Remove the cart itself
            _context.Carts.Remove(cart);
            
            Console.WriteLine($"Removing entire cart {cart.CartId} for member {memberId} during clear operation");
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddItemToCartAsync(int memberId, int productId, int quantity, decimal unitPrice)
        {
            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
            {
                cart = new Cart
                {
                    MemberId = memberId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if item already exists in cart
            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                // Update existing item
                cartItem.Quantity += quantity;
                cartItem.TotalPrice = cartItem.UnitPrice * cartItem.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new item
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * quantity,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int memberId, int productId, int quantity)
        {
            try 
            {
                Console.WriteLine($"REPOSITORY DEBUG: Updating cart item - MemberID: {memberId}, ProductID: {productId}, New Quantity: {quantity}");
                
                // Find the cart with eager loading of cart items
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.MemberId == memberId);

                if (cart == null)
                {
                    Console.WriteLine($"REPOSITORY DEBUG: Cart not found for member {memberId}");
                    return false;
                }

                // Find the cart item
                var cartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem == null)
                {
                    Console.WriteLine($"REPOSITORY DEBUG: CartItem not found for product {productId}");
                    return false;
                }

                // First, detach all entities to avoid tracking issues
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }

                Console.WriteLine($"REPOSITORY DEBUG: Found cart item - Current Quantity: {cartItem.Quantity}, UnitPrice: {cartItem.UnitPrice}, Current TotalPrice: {cartItem.TotalPrice}");

                if (quantity <= 0)
                {
                    Console.WriteLine($"REPOSITORY DEBUG: Removing cart item as quantity is {quantity}");
                    // Reattach and delete
                    _context.CartItems.Attach(cartItem);
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    // Calculate the new total price
                    decimal newTotalPrice = cartItem.UnitPrice * quantity;
                    
                    // Use direct SQL to update the cart item (most reliable)
                    string updateSql = @"
                        UPDATE CartItems 
                        SET Quantity = @Quantity, 
                            TotalPrice = @TotalPrice,
                            UpdatedAt = @UpdatedAt
                        WHERE CartId = @CartId AND ProductId = @ProductId";
                    
                    var parameters = new object[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@Quantity", quantity),
                        new Microsoft.Data.SqlClient.SqlParameter("@TotalPrice", newTotalPrice),
                        new Microsoft.Data.SqlClient.SqlParameter("@UpdatedAt", DateTime.UtcNow),
                        new Microsoft.Data.SqlClient.SqlParameter("@CartId", cart.CartId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductId", productId)
                    };
                    
                    Console.WriteLine($"REPOSITORY DEBUG: Executing direct SQL update - CartId: {cart.CartId}, " + 
                        $"ProductId: {productId}, New Quantity: {quantity}, New TotalPrice: {newTotalPrice}");
                    
                    int rowsAffected = await _context.Database.ExecuteSqlRawAsync(updateSql, parameters);
                    Console.WriteLine($"REPOSITORY DEBUG: SQL update completed with {rowsAffected} rows affected");
                    
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("REPOSITORY DEBUG: Warning - SQL update affected 0 rows!");
                        
                        // Fallback to EF Core update
                        Console.WriteLine("REPOSITORY DEBUG: Trying EF Core update as fallback");
                        
                        // Get a fresh copy of the cart item
                        var freshCartItem = await _context.CartItems
                            .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
                        
                        if (freshCartItem != null)
                        {
                            freshCartItem.Quantity = quantity;
                            freshCartItem.TotalPrice = newTotalPrice;
                            freshCartItem.UpdatedAt = DateTime.UtcNow;
                            _context.Entry(freshCartItem).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            Console.WriteLine("REPOSITORY DEBUG: EF Core update completed");
                        }
                    }
                }

                // Update cart's timestamp using direct SQL for reliability
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Carts SET UpdatedAt = @UpdatedAt WHERE CartId = @CartId",
                    new Microsoft.Data.SqlClient.SqlParameter("@UpdatedAt", DateTime.UtcNow),
                    new Microsoft.Data.SqlClient.SqlParameter("@CartId", cart.CartId)
                );
                
                // Verify the update by retrieving the cart item again from the database
                if (quantity > 0)
                {
                    // Use direct SQL for verification to bypass any caching
                    string verifySQL = @"
                        SELECT Quantity, TotalPrice 
                        FROM CartItems 
                        WHERE CartId = @CartId AND ProductId = @ProductId";
                    
                    var parameters = new object[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@CartId", cart.CartId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductId", productId)
                    };
                    
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = verifySQL;
                        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@CartId", cart.CartId));
                        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@ProductId", productId));
                        
                        if (command.Connection.State != System.Data.ConnectionState.Open)
                            await command.Connection.OpenAsync();
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int verifiedQuantity = reader.GetInt32(0);
                                decimal verifiedTotal = reader.GetDecimal(1);
                                
                                Console.WriteLine($"REPOSITORY DEBUG: Verification - Database has Quantity={verifiedQuantity}, TotalPrice={verifiedTotal}");
                                
                                if (verifiedQuantity != quantity)
                                {
                                    Console.WriteLine($"REPOSITORY DEBUG: CRITICAL ERROR - Database verification shows quantity mismatch: Expected {quantity}, got {verifiedQuantity}");
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"REPOSITORY DEBUG: Error updating cart item - {ex.Message}");
                throw; // Rethrow to be handled by caller
            }
        }

        public async Task<bool> RemoveItemFromCartAsync(int memberId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
                return false;

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
                return false;

            // Check if this is the last item BEFORE removing it
            bool isLastItem = cart.CartItems.Count <= 1;
            Console.WriteLine($"Removing item from cart: CartId={cart.CartId}, ProductId={productId}, IsLastItem={isLastItem}, ItemCount={cart.CartItems.Count}");

            // Remove the cart item
            _context.CartItems.Remove(cartItem);
            
            // If this was the last item, remove the cart too
            if (isLastItem)
            {
                Console.WriteLine($"Removing entire cart {cart.CartId} as it has no more items");
                _context.Carts.Remove(cart);
            }
            else
            {
                // Otherwise just update the timestamp
                cart.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            
            // Verify the cart was deleted if it was the last item
            if (isLastItem)
            {
                var cartCheck = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cart.CartId);
                Console.WriteLine($"Cart deletion verification: CartId={cart.CartId}, Exists={cartCheck != null}");
                
                // If cart still exists somehow, try to delete it again
                if (cartCheck != null)
                {
                    Console.WriteLine($"Cart {cart.CartId} still exists after deletion attempt, trying again");
                    _context.Carts.Remove(cartCheck);
                    await _context.SaveChangesAsync();
                }
            }
            
            return true;
        }


        
    }
} 
