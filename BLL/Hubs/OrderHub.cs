using Microsoft.AspNetCore.SignalR;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Hubs
{
    public class OrderHub : Hub
    {
        private readonly IOrderService _orderService;

        public OrderHub(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Gửi thông báo cập nhật đơn hàng
        public async Task SendOrderUpdate(List<Order> orders)
        {
            if (orders == null || orders.Count == 0)
            {
                // Optionally log this situation
                return;
            }
            await Clients.All.SendAsync("OrderUpdated", orders);
        }

        // Notify clients when an order is created
        public async Task NotifyOrderCreation(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order != null)
                {
                    await Clients.All.SendAsync("OrderCreated", orderId); // Gửi thông báo với orderId
                }
                else
                {
                    Console.WriteLine($"Order not found with ID: {orderId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while notifying order creation: {ex.Message}");
            }
        }
        public async Task NotifySpecificOrderUpdate(int orderId)
        {
            try
            {
                
                    // Gửi thông báo đến client với orderId cụ thể
                    await Clients.All.SendAsync("SpecificOrderUpdated", orderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while notifying specific order update: {ex.Message}");
            }
        }
    }
}
