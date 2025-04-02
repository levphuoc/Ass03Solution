using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Hubs
{
    public class MemberHub : Hub
    {
        public async Task SendUpdate()
        {
            await Clients.All.SendAsync("ReceiveUpdate");
        }
    }
}
