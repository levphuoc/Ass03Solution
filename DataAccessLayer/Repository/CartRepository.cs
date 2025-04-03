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

        public async Task<List<CartDetail>> GetCartItemsByCartIdAsync(int userId)
        {
            
            var cart = await _context.Carts.FirstOrDefaultAsync(n => n.MemberId == userId);

           
            if (cart == null)
            {
               
                return new List<CartDetail>(); 
            }

            var cartDetails = await _context.CartDetails
                                            .Where(ci => ci.CartId == cart.CartId)
                                            .Include(ci => ci.Product)
                                            .ToListAsync();

            if (cartDetails == null || !cartDetails.Any()) 
            {
               
                return new List<CartDetail>();
            }

            return cartDetails; 
        }
        public async Task DeleteCartAndItemsByMemberIdAsync(int memberId)
        {
            
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.MemberId == memberId);

            if (cart == null)
            {
                
                return;
            }

            int cartId = cart.CartId;
         

           
            var cartItems = await _context.CartDetails.Where(ci => ci.CartId == cartId).ToListAsync();

            if (cartItems.Any())
            {
               
                foreach (var cartItem in cartItems)
                {
                    _context.CartDetails.Remove(cartItem);  
                }
            }
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

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
