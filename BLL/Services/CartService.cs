using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJSRuntime _jsRuntime;
        private readonly IProductService _productService;
        private readonly ILogger<CartService> _logger;

        public CartService(IJSRuntime jsRuntime, IProductService productService, 
                          IUnitOfWork unitOfWork, ILogger<CartService> logger = null)
        {
            _jsRuntime = jsRuntime;
            _productService = productService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Lấy thông tin giỏ hàng từ DB
        public async Task<CartDTO> GetCartAsync(int memberId)
        {
            try
            {
                var cart = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                
                if (cart == null)
                {
                    return new CartDTO();
                }

                var cartDto = new CartDTO
                {
                    Items = cart.CartItems.Select(ci => new CartItemDTO
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.ProductName,
                        ProductImage = ci.Product.UrlImage ?? "",
                        UnitPrice = ci.UnitPrice,
                        Quantity = ci.Quantity,
                        CategoryName = ci.Product.Category?.CategoryName ?? ""
                    }).ToList()
                };

                return cartDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Lấy danh sách CartItem từ DB
        public async Task<List<CartItem>> GetCartItemsAsync(int memberId)
        {
            try
            {
                return await _unitOfWork.Carts.GetCartItemsByCartIdAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching cart items for member {memberId}");
                return new List<CartItem>();
            }
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                    quantity = 1;

                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger?.LogWarning($"Cannot add to cart - Product {productId} not found");
                    return new CartDTO();
                }

                await _unitOfWork.Carts.AddItemToCartAsync(
                    memberId, 
                    product.ProductId, 
                    quantity, 
                    product.UnitPrice);
                
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error adding product {productId} to cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity)
        {
            try
            {
                await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, quantity);
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating product {productId} quantity in cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Xóa một sản phẩm khỏi giỏ hàng
        public async Task<CartDTO> RemoveFromCartAsync(int memberId, int productId)
        {
            try
            {
                await _unitOfWork.Carts.RemoveItemFromCartAsync(memberId, productId);
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error removing product {productId} from cart for member {memberId}");
                return new CartDTO();
            }
        }

        // Xóa toàn bộ giỏ hàng
        public async Task<bool> ClearCartAsync(int memberId)
        {
            try
            {
                return await _unitOfWork.Carts.ClearCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error clearing cart for member {memberId}");
                return false;
            }
        }

        // Xóa giỏ hàng khi đã tạo đơn hàng thành công
        public async Task DeleteCartAfterOrderCreateAsync(int memberId)
        {
            try
            {
                await _unitOfWork.Carts.ClearCartAsync(memberId);
                _logger?.LogInformation($"Cart cleared for member {memberId} after order creation");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error clearing cart for member {memberId} after order creation");
            }
        }
    }
} 
