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
using Microsoft.EntityFrameworkCore;

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

        public async Task<(IEnumerable<Order> Items, int TotalCount)> GetFilteredOrdersAsync(
     int pageNumber,
     int pageSize,
     string? searchText = null,
     DateTime? orderDate = null,
     string? status = null,
     int? memberId = null)
        {
            // Lấy toàn bộ Orders từ Repository
            var orders = await _unitOfWork.Orders.GetAllAsync(); // Lấy dữ liệu từ Repository dưới dạng IEnumerable

            // Chuyển dữ liệu sang IQueryable để có thể lọc
            var query = orders.AsQueryable();

            // Filter by MemberId if provided
            if (memberId.HasValue)
            {
                query = query.Where(o => o.MemberId == memberId.Value);
            }

            // Tìm kiếm theo OrderId hoặc Freight
            if (!string.IsNullOrEmpty(searchText))
            {
                if (int.TryParse(searchText, out int orderId))
                {
                    query = query.Where(o => o.OrderId == orderId);
                }
                else if (decimal.TryParse(searchText, out decimal freight))
                {
                    query = query.Where(o => o.Freight == freight);
                }
            }

            // Lọc theo ngày OrderDate
            if (orderDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date == orderDate.Value.Date || o.RequiredDate == orderDate.Value.Date || o.ShippedDate == orderDate.Value.Date);
            }

            // Lọc theo trạng thái OrderStatus
            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
            }

            // Sắp xếp theo ngày OrderDate mới nhất
            query = query.OrderByDescending(o => o.OrderDate);

            // Lấy tổng số lượng bản ghi sau khi lọc
            var totalCount = query.Count();

            // Thực hiện phân trang
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
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
            await _cartService.DeleteCartAfterOrderCreateAsync(dto.MemberId);
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
                ShippedDate = order.ShippedDate ?? default(DateTime?),
                Freight = (decimal)order.Freight
            };
        }

        public async Task<Order> GetOrderEntityByIdAsync(int orderId)
        {
            return await _unitOfWork.Orders.GetByIdAsync(orderId);
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
           var order =  await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");
            order.ShippedDate = DateTime.Now;
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task ShippedOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Shipped);
        }
        public async Task CancelOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Cancel);
        }
        public async Task<List<Order>> GetPagedOrdersStaffAsync(int pageNumber, int pageSize, string status)
        {
            // Allow staff to see all order statuses
            Expression<Func<Order, bool>> predicate;

            if (status == "ALL")
            {
                // Return all orders regardless of status
                predicate = o => true;
            }
            else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                // Filter by the specific status
                predicate = o => o.Status == parsedStatus;
            }
            else
            {
                // Invalid status - return empty list
                return new List<Order>();
            }

            // Use the existing GetPagedAsync method
            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                query => query.OrderByDescending(o => o.OrderDate)
            );

            // Load members separately if needed
            var ordersList = orders.ToList();
            foreach (var order in ordersList)
            {
                if (order.MemberId > 0)
                {
                    order.Member = await _unitOfWork.Members.GetByIdAsync(order.MemberId);
                }
            }

            return ordersList;
        }

        public async Task<List<Order>> GetPagedOrdersShipperAsync(int pageNumber, int pageSize, string status)
        {
            Console.WriteLine($"GetPagedOrdersShipperAsync called with status: {status}");
            
            var validStatuses = new List<OrderStatus>
            {
                OrderStatus.Approve,
                OrderStatus.Shipping,
                OrderStatus.Shipped
            };

            // Trước tiên lấy tất cả đơn hàng để kiểm tra
            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            Console.WriteLine($"Total orders in database: {allOrders.Count()}");
            
            // In trạng thái của tất cả đơn hàng để debug
            foreach (var order in allOrders)
            {
                Console.WriteLine($"DB Order {order.OrderId}: Status = {order.Status} ({(int)order.Status})");
            }

            Expression<Func<Order, bool>> predicate;

            if (status == "ALL")
            {
                // Lọc để lấy các đơn hàng có trạng thái Approve, Shipping, hoặc Shipped
                predicate = o => o.Status == OrderStatus.Approve || 
                                o.Status == OrderStatus.Shipping || 
                                o.Status == OrderStatus.Shipped;
                Console.WriteLine("Using ALL status filter for Shipper: Approve, Shipping, Shipped");
            }
            else if (int.TryParse(status, out int statusId))
            {
                // Nếu status là số, chuyển đổi thành OrderStatus và kiểm tra
                if (Enum.IsDefined(typeof(OrderStatus), statusId))
                {
                    OrderStatus parsedStatus = (OrderStatus)statusId;
                    
                    if (validStatuses.Contains(parsedStatus))
                    {
                        predicate = o => o.Status == parsedStatus;
                        Console.WriteLine($"Filtering by status ID: {statusId} ({parsedStatus})");
                    }
                    else
                    {
                        Console.WriteLine($"Status ID {statusId} not in valid statuses");
                        return new List<Order>();
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid status ID: {statusId}");
                    return new List<Order>();
                }
            }
            else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus) && validStatuses.Contains(parsedStatus))
            {
                predicate = o => o.Status == parsedStatus;
                Console.WriteLine($"Filtering by parsed status: {parsedStatus}");
            }
            else
            {
                Console.WriteLine($"Could not parse status: {status}");
                return new List<Order>();
            }

            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                query => query.OrderByDescending(o => o.OrderDate)
            );
            
            Console.WriteLine($"Found {orders.Count()} orders for Shipper view");
            foreach (var order in orders)
            {
                Console.WriteLine($"Result Order {order.OrderId}: Status = {order.Status} ({(int)order.Status})");
            }

            return orders.ToList();
        }




    }
}
