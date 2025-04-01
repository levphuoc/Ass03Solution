﻿using BLL.DTOs;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IOrderService
    {
        Task<int> GetTotalOrdersAsync();
        Task<List<Order>> GetPagedOrdersAsync(int pageNumber, int pageSize);
        Task<int> CreateOrderAsync(OrderDTO dto);
        Task<OrderDTO> GetOrderByIdAsync(int orderId); 
        Task UpdateOrderAsync(OrderDTO order);
        Task DeleteOrderAsync(int orderId);
    }
}
