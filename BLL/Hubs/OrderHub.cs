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

       
        public async Task NotifyOrderCreation(int orderId)
        {
           
            
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order != null)
                {
                    await Clients.All.SendAsync("OrderCreated", orderId); 
                }
                else
                {
                    Console.WriteLine($"Order not found with ID: {orderId}");
                }
           
        }
        public async Task NotifySpecificOrderUpdate(int orderId)
        {
           
                    await Clients.All.SendAsync("SpecificOrderUpdated", orderId);
           
        }
    }
}
