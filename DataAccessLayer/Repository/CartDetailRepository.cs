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
    public class CartDetailRepository : GenericRepository<CartItem>, ICartDetailRepository
    {
        private new readonly EStoreDbContext _context;

        public CartDetailRepository(EStoreDbContext context) : base(context)
        {
            _context = context;
        }

        
        
    }
}
