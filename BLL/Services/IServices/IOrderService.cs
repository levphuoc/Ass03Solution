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
        Task<(IEnumerable<Order> Items, int TotalCount)> GetFilteredOrdersAsync(
     int pageNumber,
     int pageSize,
     string? searchText = null,
     DateTime? orderDate = null,
     string? status = null,
     int? memberId = null);
        Task<int> CreateOrderAsync(OrderDTO dto);
        Task<OrderDTO> GetOrderByIdAsync(int orderId); 
        Task<Order> GetOrderEntityByIdAsync(int orderId);
        Task UpdateOrderAsync(OrderDTO order);
        Task DeleteOrderAsync(int orderId);
        Task ApproveOrderAsync(int orderId);
        Task CancelOrderAsync(int orderId);
        Task<List<Order>> GetPagedOrdersStaffAsync(int pageNumber, int pageSize, string status);
        Task<List<Order>> GetPagedOrdersShipperAsync(int pageNumber, int pageSize, string status);
        Task RejectOrderAsync(int orderId);


        Task ShippingOrderAsync(int orderId);

        Task ShippedOrderAsync(int orderId);

    }
}
