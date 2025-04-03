using BLL.Services.IServices;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Hubs
{
    public class SalesReportHub : Hub
    {
        private readonly ISalesReportService _reportService;

        public SalesReportHub(ISalesReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task TriggerReport(DateTime startDate, DateTime endDate)
        {
            await _reportService.GenerateReportAsync(startDate, endDate);
        }
      
    }


}
