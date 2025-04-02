﻿using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.FirebaseServices;
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
        private readonly eStoreDbContext _context;
        private readonly IHubContext<SalesReportHub> _hubContext;
        private readonly IFirebaseDataUploaderService _firebaseUploader;

        public SalesReportService(
            eStoreDbContext context,
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
                await _firebaseUploader.UploadSalesReportWithSubcollectionAsync( startDate, endDate, report);
                // Send to UI via SignalR
                await _hubContext.Clients.All.SendAsync("SalesReportGenerated", report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Firebase upload failed: {ex.Message}");
                // Optional: Log this to your own logger
            }

            return report;
        }
    }

}
