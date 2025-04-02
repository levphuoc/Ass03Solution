using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Data;
using DataAccessLayer.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class SalesReportService : ISalesReportService
    {
        private readonly EStoreDbContext _context;
        private readonly IHubContext<SalesReportHub> _hubContext;

        public SalesReportService(EStoreDbContext context, IHubContext<SalesReportHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
            
        public async Task<List<SalesReportDTO>> GenerateReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new SalesReportDTO
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity * (1 - (decimal)x.Discount))
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToListAsync();

            // Push notification to all clients (real-time update)
            await _hubContext.Clients.All.SendAsync("SalesReportGenerated", report);

            return report;
        }
    }

}
