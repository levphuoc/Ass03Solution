using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace BLL.Hubs
{
    public class ProductHub : Hub
    {
        public async Task NotifyProductCreated(Product product)
        {
            await Clients.All.SendAsync("ProductCreated", product);
        }

        public async Task NotifyProductUpdated(Product product)
        {
            await Clients.All.SendAsync("ProductUpdated", product);
        }

        public async Task NotifyProductDeleted(int productId)
        {
            await Clients.All.SendAsync("ProductDeleted", productId);
        }
    }
} 