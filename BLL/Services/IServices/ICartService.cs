﻿using DataAccessLayer.Entities;
using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface ICartService
    {
        Task<List<CartDetail>> GetAllCartDetailById(int userId);
        Task DeleteCartAndItemsByUserIdAsync(int MemberId);
        Task<CartDTO> GetCartAsync(int memberId, string role);
        Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity, string role);
        Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity, string role);
        Task<CartDTO> RemoveFromCartAsync(int memberId, int productId, string role);
        Task<bool> ClearCartAsync(int memberId, string role);
    }
} 
