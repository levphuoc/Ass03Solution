using BLL.DTOs;
using DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface ITrackingOrderService
    {
        Task AddTracingOrderAsync(TrackingOrderDTO tracingOrderDto);
        Task<List<TrackingOrderDTO>> GetPagedTrackingOrdersAsync(int pageNumber, int pageSize, string status);
        Task<int> GetTotalOrdersAsync();
        Task UpdateTrackingOrderAsync(int orderId, OrderStatus newStatus);



    }
}
