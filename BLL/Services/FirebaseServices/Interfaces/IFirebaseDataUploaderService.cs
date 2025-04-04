using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.FirebaseServices.Interfaces
{
    public interface IFirebaseDataUploaderService
    {
        Task UploadReportWithProductsAsync(DateTime startDate, DateTime endDate, List<SalesReportDTO> reportData);
    }
}
