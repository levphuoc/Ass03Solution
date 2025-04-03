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

            _context.CartItems.RemoveRange(cart.CartItems);
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
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
                return false;

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
                return false;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.TotalPrice = cartItem.UnitPrice * quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
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

            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }


        
    }
} 
