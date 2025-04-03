
using AutoMapper;
using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Enum;
using DataAccessLayer.UnitOfWork;
using System.Linq.Expressions;
namespace BLL.Services
{
   

        public class TracingOrderService : ITrackingOrderService
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public TracingOrderService(IUnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task AddTracingOrderAsync(TrackingOrderDTO tracingOrderDto)
            {
                var tracingOrder = _mapper.Map<TracingOrder>(tracingOrderDto);
                await _unitOfWork.TrackingOrders.AddAsync(tracingOrder);
                await _unitOfWork.SaveChangesAsync();
            }

        public async Task<List<TrackingOrderDTO>> GetPagedTrackingOrdersAsync(int pageNumber, int pageSize, string status)
        {
            OrderStatus? orderStatus = null;
            if (status != "ALL" && Enum.TryParse(status, true, out OrderStatus parsedStatus))
            {
                orderStatus = parsedStatus;
            }

            var predicate = orderStatus == null
                ? (Expression<Func<TracingOrder, bool>>)(_ => true)
                : (Expression<Func<TracingOrder, bool>>)(o => o.Status == orderStatus);

            var (tracingOrders, totalCount) = await _unitOfWork.TrackingOrders.GetPagedAsync(
                pageNumber, pageSize, predicate, query => query.OrderByDescending(o => o.UpdatedAt)
            );

            // Map từ TracingOrder sang TrackingOrderDTO
            var trackingOrderDTOs = _mapper.Map<List<TrackingOrderDTO>>(tracingOrders);

            return trackingOrderDTOs;
        }
        public async Task<int> GetTotalOrdersAsync()
        {
            var orders = await _unitOfWork.TrackingOrders.GetAllAsync();
            return orders.Count();
        }
        public async Task UpdateTrackingOrderAsync(int orderId, OrderStatus newStatus)
        {
            var trackingOrder = await _unitOfWork.TrackingOrders.FirstOrDefaultAsync(t => t.OrderId == orderId);

            if (trackingOrder == null)
            {
                throw new KeyNotFoundException("Tracking Order not found.");
            }

            trackingOrder.Status = newStatus;
            trackingOrder.UpdatedAt = DateTime.Now;

            await _unitOfWork.TrackingOrders.UpdateAsync(trackingOrder);
            await _unitOfWork.SaveChangesAsync();
        }
    }
    

}
