using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        //    Task<OrderDetail> GetOrderDetail(int orderId, int productId);
        //    Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderId(int orderId);
        //    Task AddAsync(OrderDetail orderDetail);
        //    Task UpdateAsync(OrderDetail orderDetail);
        //    Task DeleteAsync(int orderId, int productId);
    }
}
