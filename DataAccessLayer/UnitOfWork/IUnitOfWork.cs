using DataAccessLayer.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMemberRepository Members { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }
        ICartRepository Carts { get; }
        ITrackingOrderRepository TrackingOrders { get; }
        ICategoryRepository Categories { get; }
        
        Task<int> SaveChangesAsync();
        
        // Get direct database connection for emergency operations
        DbConnection GetDbConnection();
        
        // Get database context for direct SQL operations
        Microsoft.EntityFrameworkCore.DbContext GetDbContext();
    }
}
