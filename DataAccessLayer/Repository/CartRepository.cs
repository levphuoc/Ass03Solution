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



    }
}
