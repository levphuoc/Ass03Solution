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
using Microsoft.EntityFrameworkCore;

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
                    return new CartDTO { ErrorMessage = "Product not found." };
                }
                
                // Check if product is in stock
                if (product.UnitsInStock <= 0)
                {
                    _logger?.LogWarning($"Cannot add to cart - Product {productId} is out of stock");
                    return new CartDTO { ErrorMessage = $"Product '{product.ProductName}' is out of stock." };
                }
                
                // Check if requested quantity is available
                if (product.UnitsInStock < quantity)
                {
                    _logger?.LogWarning($"Cannot add requested quantity - Product {productId} has only {product.UnitsInStock} units in stock");
                    return new CartDTO { 
                        ErrorMessage = $"Only {product.UnitsInStock} units of '{product.ProductName}' are available." 
                    };
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
                return new CartDTO { ErrorMessage = "An error occurred while adding the product to cart." };
            }
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity)
        {
            try
            {
                // If quantity is 0 or negative, remove the item instead
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(memberId, productId);
                }
                
                // Check product stock availability
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger?.LogWarning($"Cannot update cart - Product {productId} not found");
                    return new CartDTO { ErrorMessage = "Product not found." };
                }
                
                // Check if product is in stock
                if (product.UnitsInStock <= 0)
                {
                    _logger?.LogWarning($"Cannot update cart - Product {productId} is out of stock");
                    // Remove the item from cart since it's out of stock
                    await RemoveFromCartAsync(memberId, productId);
                    return new CartDTO { ErrorMessage = $"Product '{product.ProductName}' has been removed from your cart because it is out of stock." };
                }
                
                // Check if requested quantity is available
                if (product.UnitsInStock < quantity)
                {
                    _logger?.LogWarning($"Cannot update to requested quantity - Product {productId} has only {product.UnitsInStock} units available");
                    // Update to maximum available quantity instead
                    await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, product.UnitsInStock);
                    var cart = await GetCartAsync(memberId);
                    cart.ErrorMessage = $"Only {product.UnitsInStock} units of '{product.ProductName}' are available. Your cart has been updated.";
                    return cart;
                }

                await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, quantity);
                return await GetCartAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating product {productId} quantity in cart for member {memberId}");
                return new CartDTO { ErrorMessage = "An error occurred while updating the cart." };
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

        // Add a private method for direct SQL deletion
        private async Task ForceDeleteCartWithSQLAsync(int memberId, int cartId)
        {
            try
            {
                _logger?.LogWarning($"Using direct SQL to force delete cart {cartId} for member {memberId}");
                
                // Get a database connection from the UnitOfWork
                var connection = _unitOfWork.GetDbConnection();
                
                // Delete cart items first
                using (var command1 = connection.CreateCommand())
                {
                    command1.CommandText = $"DELETE FROM CartItems WHERE CartId = {cartId}";
                    int itemsDeleted = await command1.ExecuteNonQueryAsync();
                    _logger?.LogInformation($"Direct SQL: Deleted {itemsDeleted} items for cart {cartId}");
                }
                
                // Then delete the cart itself
                using (var command2 = connection.CreateCommand())
                {
                    command2.CommandText = $"DELETE FROM Carts WHERE CartId = {cartId}";
                    int cartsDeleted = await command2.ExecuteNonQueryAsync();
                    _logger?.LogInformation($"Direct SQL: Deleted {cartsDeleted} carts with ID {cartId}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in direct SQL deletion: {ex.Message}");
                // Try the absolute last resort approach - use raw SQL through Entity Framework
                try
                {
                    // Get access to the context through reflection if necessary
                    // This is just for debugging purposes to ensure we can delete the cart
                    var dbContext = _unitOfWork.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_unitOfWork) as DbContext;
                    
                    if (dbContext != null)
                    {
                        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM CartItems WHERE CartId = {cartId}");
                        await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM Carts WHERE CartId = {cartId}");
                        _logger?.LogInformation($"Last resort SQL through EF Core: Attempted to delete cart {cartId}");
                    }
                }
                catch (Exception innerEx)
                {
                    _logger?.LogError(innerEx, $"Last resort EF Core deletion failed: {innerEx.Message}");
                }
            }
        }

        // Xóa giỏ hàng khi đã tạo đơn hàng thành công
        public async Task DeleteCartAfterOrderCreateAsync(int memberId)
        {
            try
            {
                // First, check if cart exists
                var cart = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cart == null)
                {
                    _logger?.LogInformation($"No cart found for member {memberId} to delete after order creation");
                    return;
                }

                // Log details for debugging
                int cartId = cart.CartId;
                int itemCount = cart.CartItems?.Count ?? 0;
                _logger?.LogInformation($"Starting to delete cart ID {cartId} with {itemCount} items for member {memberId}");
                
                // Delete both cart and cart items completely
                await _unitOfWork.Carts.DeleteCartAndItemsByMemberIdAsync(memberId);
                
                // Verify deletion
                var cartAfterDeletion = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                if (cartAfterDeletion != null)
                {
                    _logger?.LogWarning($"Failed to delete cart for member {memberId} - cart still exists after deletion attempt");
                    
                    // Try a second time with a different approach - manual deletion of cart items first, then cart
                    try 
                    {
                        // First clear all cart items
                        await _unitOfWork.Carts.ClearCartAsync(memberId);
                        
                        // Then try to find and delete the cart directly through the generic repository
                        var cartsToDelete = await _unitOfWork.Carts.FindAsync(c => c.MemberId == memberId);
                        foreach (var cartToDelete in cartsToDelete)
                        {
                            await _unitOfWork.Carts.DeleteAsync(cartToDelete.CartId);
                        }
                        
                        // Save changes to ensure everything is committed
                        await _unitOfWork.SaveChangesAsync();
                        
                        _logger?.LogInformation($"Second attempt: Manually deleted cart for member {memberId}");
                        
                        // Verify again
                        cartAfterDeletion = await _unitOfWork.Carts.GetCartWithItemsByMemberIdAsync(memberId);
                        if (cartAfterDeletion != null)
                        {
                            // Third attempt - direct SQL as a last resort
                            await ForceDeleteCartWithSQLAsync(memberId, cartId);
                        }
                    }
                    catch (Exception innerEx) 
                    {
                        _logger?.LogError(innerEx, $"Second attempt: Error manually deleting cart for member {memberId}");
                        // Try direct SQL as a last resort
                        await ForceDeleteCartWithSQLAsync(memberId, cartId);
                    }
                }
                else
                {
                    _logger?.LogInformation($"Cart and items successfully deleted for member {memberId} after order creation");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting cart for member {memberId} after order creation: {ex.Message}");
            }
        }
    }
} 
