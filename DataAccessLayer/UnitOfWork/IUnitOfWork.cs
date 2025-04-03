using DataAccessLayer.Repository.Interfaces;
using System;
using System.Collections.Generic;
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
    }
}
