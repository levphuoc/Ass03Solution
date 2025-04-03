using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Enum;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITrackingOrderService _trackingOrderService;
        private readonly ICartService _cartService;
        private readonly IHubContext<OrderHub> _hubContext;
        public OrderService(IUnitOfWork unitOfWork, ITrackingOrderService trackingOrderService, ICartService cartService, IHubContext<OrderHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _trackingOrderService = trackingOrderService;
            _cartService = cartService;
            _hubContext = hubContext;
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders.Count();
        }



        public async Task<List<Order>> GetPagedOrdersAsync(int pageNumber, int pageSize)
        {
            
            Expression<Func<Order, bool>> predicate = _ => true;  

            
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = query => query.OrderByDescending(o => o.OrderDate);

           
            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(pageNumber, pageSize, predicate, orderBy);

           
            return orders.ToList();
        }
        public async Task<List<Order>> GetPagedOrdersAsync(int pageNumber, int pageSize, string status)
        {
            
            OrderStatus? orderStatus = null;
            if (status != "ALL" && Enum.TryParse(status, true, out OrderStatus parsedStatus))
            {
                orderStatus = parsedStatus;
            }

         
            var predicate = orderStatus == null
                ? (Expression<Func<Order, bool>>)(_ => true)
                : (Expression<Func<Order, bool>>)(o => o.Status == orderStatus);

            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(pageNumber, pageSize, predicate, query => query.OrderByDescending(o => o.OrderDate));

            return orders.ToList();
        }



        public async Task<int> CreateOrderAsync(OrderDTO dto)
        {
            var order = new Order
            {
                MemberId = dto.MemberId,
                OrderDate = dto.OrderDate,
                RequiredDate = dto.RequiredDate,
                ShippedDate = dto.ShippedDate,
                Freight = dto.Freight
            };

            order.Status = DataAccessLayer.Enum.OrderStatus.Spending;
          
            await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.SaveChangesAsync();
            var trackingOrderDto = new TracingOrder
            {
                OrderId = order.OrderId,
                MemberId = order.MemberId,
                Status = order.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            
            await _trackingOrderService.AddTracingOrderAsync(trackingOrderDto);
            await _cartService.DeleteCartAndItemsByUserIdAsync(dto.MemberId);
            return order.OrderId;
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                return null;

            return new OrderDTO
            {
                OrderId = order.OrderId,
                MemberId = order.MemberId,
                OrderDate = order.OrderDate,
                RequiredDate = (DateTime)order.RequiredDate,
                ShippedDate = (DateTime)order.ShippedDate,
                Freight = (decimal)order.Freight
            };
        }

        public async Task UpdateOrderAsync(OrderDTO orderDTO)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync((int)orderDTO.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // Cập nhật dữ liệu
            order.MemberId = orderDTO.MemberId;
            order.OrderDate = orderDTO.OrderDate;
            order.RequiredDate = orderDTO.RequiredDate;
            order.ShippedDate = orderDTO.ShippedDate;
            order.Freight = orderDTO.Freight;

             await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteOrderAsync(int orderId)
        {
            await _unitOfWork.Orders.DeleteAsync(orderId);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
           
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null) throw new KeyNotFoundException("Order not found.");      
            order.Status = newStatus;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await _trackingOrderService.UpdateTrackingOrderAsync(orderId, newStatus);
            await _hubContext.Clients.All.SendAsync("ReceiveStatusChange", orderId, newStatus.ToString());
        }

        public async Task ApproveOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Approve);
        }

        public async Task RejectOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Reject);
        }

        public async Task ShippingOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Shipping);
        }

        public async Task ShippedOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Shipped);
        }
        public async Task<List<Order>> GetPagedOrdersStaffAsync(int pageNumber, int pageSize, string status)
        {
            var validStatuses = new List<OrderStatus>
    {
        OrderStatus.Spending,
        OrderStatus.Approve,
        OrderStatus.Reject
    };

            Expression<Func<Order, bool>> predicate;

            if (status == "ALL")
            {
                // Chỉ lấy các trạng thái trong danh sách hợp lệ (Spending, Approve, Reject)
                predicate = o => validStatuses.Contains(o.Status);
            }
            else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus) && validStatuses.Contains(parsedStatus))
            {
                predicate = o => o.Status == parsedStatus;
            }
            else
            {
                // Nếu không hợp lệ hoặc không nằm trong validStatuses, trả về danh sách rỗng
                return new List<Order>();
            }

            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                query => query.OrderByDescending(o => o.OrderDate)
            );

            return orders.ToList();
        }

        public async Task<List<Order>> GetPagedOrdersShipperAsync(int pageNumber, int pageSize, string status)
        {
            
            var validStatuses = new List<OrderStatus>
    {
        OrderStatus.Approve,
        OrderStatus.Shipping,
        OrderStatus.Shipped
    };

            
            Expression<Func<Order, bool>> predicate;

            if (status == "ALL")
            {
                predicate = o => validStatuses.Contains(o.Status);
            }
            else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus) && validStatuses.Contains(parsedStatus))
            {
                predicate = o => o.Status == parsedStatus;
            }
            else
            {
                
                return new List<Order>();
            }

            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                query => query.OrderByDescending(o => o.OrderDate)
            );

            return orders.ToList();
        }




    }
}
