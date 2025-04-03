using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IOrderDetailService
    {
        Task AddOrderDetailsAsync(IEnumerable<OrderItemDTO> orderDetails);
        Task<List<OrderItemDTO>> GetOrderItemsByOrderIdAsync(int orderId);
        Task DeleteOrderItemsByOrderIdAsync(int orderId);
    }
}
