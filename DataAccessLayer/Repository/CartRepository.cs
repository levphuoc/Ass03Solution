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

            Console.WriteLine($"✅ CartId tìm thấy: {cart.CartId}");

            // Lấy danh sách CartDetail theo CartId
            var cartDetails = await _context.CartDetails
                                            .Where(ci => ci.CartId == cart.CartId)
                                            .Include(ci => ci.Product)
                                            .ToListAsync();

            if (cartDetails == null || !cartDetails.Any()) // Kiểm tra nếu không có sản phẩm
            {
                Console.WriteLine($"⚠ Không có sản phẩm trong CartId: {cart.CartId}");
                return new List<CartDetail>();
            }

            return cartDetails; // Trả về danh sách CartDetail nếu có sản phẩm
        }


    }
}
