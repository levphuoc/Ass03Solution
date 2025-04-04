using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.FirebaseServices;
using BLL.Services.FirebaseServices.Interfaces;
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
        private readonly IFirebaseDataUploaderService _firebaseUploader;

        public SalesReportService(
            EStoreDbContext context,
            IHubContext<SalesReportHub> hubContext,
            IFirebaseDataUploaderService firebaseUploader)
        {
            _context = context;
            _hubContext = hubContext;
            _firebaseUploader = firebaseUploader;
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

            try
            {
                if (report.Any())
                {
                    await _firebaseUploader.UploadReportWithProductsAsync(startDate, endDate, report);
                    Console.WriteLine("Report sent to Firestore.");
                }
                else
                {
                    Console.WriteLine("No sales data to upload to Firestore.");
                }

                await _hubContext.Clients.All.SendAsync("SalesReportGenerated", report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Firebase upload failed: {ex.Message}");
            }

            return report;
        }
    }


}
