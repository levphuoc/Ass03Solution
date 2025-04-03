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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(EStoreDbContext context) : base(context) { }

        public async Task<bool> HasProductsByCategoryIdAsync(int categoryId)
        {
            return await _dbSet.AnyAsync(p => p.CategoryId == categoryId);
        }
    }
}
