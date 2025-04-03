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
        Task<CartDTO> GetCartAsync(int memberId);
        Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity = 1);
        Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity);
        Task<CartDTO> RemoveFromCartAsync(int memberId, int productId);
        Task<bool> ClearCartAsync(int memberId);
    }
} 