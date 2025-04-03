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
    public class CartDetailRepository : GenericRepository<CartDetail>, ICartDetailRepository
    {
        private new readonly EStoreDbContext _context;

        public CartDetailRepository(EStoreDbContext context) : base(context)
        {
            _context = context;
        }

        
        public async Task<List<CartDetail>> GetAllCartDetailById(int userId)
        {
           
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.MemberId == userId);

            if (cart == null)
            {
                return new List<CartDetail>();
            }

          
            var cartDetails = await _context.CartDetails
                .Include(cd => cd.Product) 
                .Where(cd => cd.CartId == cart.CartId)
                .ToListAsync();

            return cartDetails; 
        }
    }
}
