using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<List<CartItem>> GetCartItemsByCartIdAsync(int userId);
        Task DeleteCartAndItemsByMemberIdAsync(int memberId);
        Task<Cart> GetCartWithItemsByMemberIdAsync(int memberId);
        Task<bool> ClearCartAsync(int memberId);
        Task<bool> AddItemToCartAsync(int memberId, int productId, int quantity, decimal unitPrice);
        Task<bool> UpdateCartItemQuantityAsync(int memberId, int productId, int quantity);
        Task<bool> RemoveItemFromCartAsync(int memberId, int productId);
        
    }
} 
