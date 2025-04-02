using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IFirebaseDataUploaderService
    {
        Task UploadSalesReportWithSubcollectionAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData);
        Task UploadSalesReportAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData);
        Task UploadSalesReportAsync(List<SalesReportDTO> data, DateTime start, DateTime end);
    }
}
