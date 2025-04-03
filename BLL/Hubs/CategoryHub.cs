using BLL.DTOs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BLL.Hubs
{
    public class CategoryHub : Hub
    {
        // This hub will be used to broadcast category updates to all connected clients
        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}. Reason: {exception?.Message ?? "None"}");
            await base.OnDisconnectedAsync(exception);
        }
        
        // Convenience methods for calling from elsewhere
        public async Task NotifyCategoryCreated(CategoryDTO category)
        {
            await Clients.All.SendAsync("CategoryCreated", category);
        }
        
        public async Task NotifyCategoryUpdated(CategoryDTO category)
        {
            await Clients.All.SendAsync("CategoryUpdated", category);
        }
        
        public async Task NotifyCategoryDeleted(int categoryId)
        {
            await Clients.All.SendAsync("CategoryDeleted", categoryId);
        }
    }
} 