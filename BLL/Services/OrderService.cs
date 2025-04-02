using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
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

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            // Trả về danh sách đơn hàng
            return orders.ToList();
        }
        public async Task<List<Order>> GetPagedOrdersAsync(int pageNumber, int pageSize, string status)
        {
            // Lấy tất cả đơn hàng nếu trạng thái là "ALL", hoặc chỉ lấy các đơn hàng có trạng thái tương ứng
            var predicate = status == "ALL" ? _ => true : (Expression<Func<Order, bool>>)(o => o.Status.ToLower() == status.ToLower());

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

            order.Status = "SPENDING";
            // Thêm Order vào cơ sở dữ liệu
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Trả về OrderId sau khi thêm thành công
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
        

    }
}
