using DataAccessLayer.Data;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EStoreDbContext _context;
        private bool _disposed = false;

        public UnitOfWork(EStoreDbContext context)
        {
            _context = context;
            Members = new MemberRepository(_context);
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
            OrderDetails = new OrderDetailRepository(_context);
        }

        public IMemberRepository Members { get; private set; }
        public IProductRepository Products { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IOrderDetailRepository OrderDetails { get; private set; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
