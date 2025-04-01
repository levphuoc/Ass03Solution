using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            return allOrders
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
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
