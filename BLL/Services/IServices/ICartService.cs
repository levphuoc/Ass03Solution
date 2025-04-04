using DataAccessLayer.Entities;
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
        // Lấy thông tin giỏ hàng
        Task<CartDTO> GetCartAsync(int memberId);
        
        // Lấy danh sách các item trong giỏ hàng
        Task<List<CartItem>> GetCartItemsAsync(int memberId);
        
        // Thêm sản phẩm vào giỏ hàng
        Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity);
        
        // Cập nhật số lượng sản phẩm trong giỏ hàng
        Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity);
        
        // Xóa một sản phẩm khỏi giỏ hàng
        Task<CartDTO> RemoveFromCartAsync(int memberId, int productId);
        
        // Xóa toàn bộ giỏ hàng
        Task<bool> ClearCartAsync(int memberId);
        
        // Xóa giỏ hàng khi đã tạo đơn hàng thành công
        Task DeleteCartAfterOrderCreateAsync(int memberId);
        
        // Xóa cart bất kể còn item hay không 
        Task<bool> ForceDeleteCartAsync(int memberId);
    }
} 
