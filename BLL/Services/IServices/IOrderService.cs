
using BLL.DTOs;
using DataAccessLayer.Entities;
using DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IOrderService
    {
        Task<int> GetTotalOrdersAsync();
        Task<List<Order>> GetPagedOrdersAsync(int pageNumber, int pageSize, string status);
        Task<int> CreateOrderAsync(OrderDTO dto);
        Task<OrderDTO> GetOrderByIdAsync(int orderId); 
        Task UpdateOrderAsync(OrderDTO order);
        Task DeleteOrderAsync(int orderId);
        Task ApproveOrderAsync(int orderId);


        Task RejectOrderAsync(int orderId);


        Task ShippingOrderAsync(int orderId);

        Task ShippedOrderAsync(int orderId);

    }
}
