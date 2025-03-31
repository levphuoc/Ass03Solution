using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface ISalesReportService
    {
        Task<List<SalesReportDTO>> GenerateReportAsync(DateTime startDate, DateTime endDate);
    }
}
