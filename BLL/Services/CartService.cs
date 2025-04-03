
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;  // Thêm namespace cho ILogger
using AutoMapper;
using BLL.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJSRuntime _jsRuntime;
        private readonly IProductService _productService;
        private readonly ILogger<CartService> _logger;
        private const string CartKey = "estore_cart_";
        private readonly ICartDetailRepository _cartDetailRepository;
        private readonly ICartRepository _cartRepository;

        public CartService(IJSRuntime jsRuntime, IProductService productService, IUnitOfWork unitOfWork, ICartDetailRepository cartDetailRepository, ICartRepository cartRepository)
        {
            _jsRuntime = jsRuntime;
            _productService = productService;
            _unitOfWork = unitOfWork;
            _cartDetailRepository = cartDetailRepository;
            _cartRepository = cartRepository;
        }

        

       
        

        public async Task<List<CartItem>> GetAllCartDetailById(int userId)
        {
            return await _cartRepository.GetCartItemsByCartIdAsync(userId);
        }

        public async Task DeleteCartAndItemsByUserIdAsync(int MemberId)
        {

            await _cartRepository.DeleteCartAndItemsByMemberIdAsync(MemberId);
        }

        private string GetCartKey(int memberId) => $"{CartKey}{memberId}";

        private bool ShouldUseLocalStorage(string role)
        {
            return role == "Admin" || role == "Staff" || role == "Member";
        }

        public async Task<CartDTO> GetCartAsync(int memberId, string role)
        {
            if (ShouldUseLocalStorage(role))
            {
                return await GetCartFromLocalStorageAsync(memberId);
            }
            else
            {
                return await GetCartFromDatabaseAsync(memberId);
            }
        }

        private async Task<CartDTO> GetCartFromLocalStorageAsync(int memberId)
        {
            try
            {
                var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", GetCartKey(memberId));
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new CartDTO();
                }

                var cart = JsonSerializer.Deserialize<CartDTO>(cartJson) ?? new CartDTO();
                return cart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching cart from localStorage: {ex.Message}");
                return new CartDTO();
            }
        }

        private async Task<CartDTO> GetCartFromDatabaseAsync(int memberId)
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
                Console.WriteLine($"Error fetching cart from database: {ex.Message}");
                return new CartDTO();
            }
        }

        public async Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity, string role)
        {
            if (quantity <= 0)
                quantity = 1;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return new CartDTO();

            if (ShouldUseLocalStorage(role))
            {
                return await AddToLocalStorageCartAsync(memberId, product, quantity);
            }
            else
            {
                return await AddToDatabaseCartAsync(memberId, product, quantity);
            }
        }

        private async Task<CartDTO> AddToLocalStorageCartAsync(int memberId, ProductDTO product, int quantity)
        {
            var cart = await GetCartFromLocalStorageAsync(memberId);
            
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existingItem != null)
            {
                // Update quantity if item already in cart
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                cart.Items.Add(new CartItemDTO
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductImage = product.UrlImage ?? "",
                    UnitPrice = product.UnitPrice,
                    Quantity = quantity,
                    CategoryName = product.CategoryName ?? ""
                });
            }

            await SaveCartToLocalStorageAsync(memberId, cart);
            return cart;
        }

        private async Task<CartDTO> AddToDatabaseCartAsync(int memberId, ProductDTO product, int quantity)
        {
            await _unitOfWork.Carts.AddItemToCartAsync(
                memberId, 
                product.ProductId, 
                quantity, 
                product.UnitPrice);
            
            return await GetCartFromDatabaseAsync(memberId);
        }

        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity, string role)
        {
            if (ShouldUseLocalStorage(role))
            {
                return await UpdateLocalStorageCartItemAsync(memberId, productId, quantity);
            }
            else
            {
                return await UpdateDatabaseCartItemAsync(memberId, productId, quantity);
            }
        }

        private async Task<CartDTO> UpdateLocalStorageCartItemAsync(int memberId, int productId, int quantity)
        {
            var cart = await GetCartFromLocalStorageAsync(memberId);
            
            var itemToUpdate = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToUpdate == null)
                return cart;

            if (quantity <= 0)
            {
                // Remove item if quantity is 0 or negative
                cart.Items.Remove(itemToUpdate);
            }
            else
            {
                // Update quantity
                itemToUpdate.Quantity = quantity;
            }

            await SaveCartToLocalStorageAsync(memberId, cart);
            return cart;
        }

        private async Task<CartDTO> UpdateDatabaseCartItemAsync(int memberId, int productId, int quantity)
        {
            await _unitOfWork.Carts.UpdateCartItemQuantityAsync(memberId, productId, quantity);
            return await GetCartFromDatabaseAsync(memberId);
        }

        public async Task<CartDTO> RemoveFromCartAsync(int memberId, int productId, string role)
        {
            if (ShouldUseLocalStorage(role))
            {
                return await RemoveFromLocalStorageCartAsync(memberId, productId);
            }
            else
            {
                return await RemoveFromDatabaseCartAsync(memberId, productId);
            }
        }

        private async Task<CartDTO> RemoveFromLocalStorageCartAsync(int memberId, int productId)
        {
            var cart = await GetCartFromLocalStorageAsync(memberId);
            
            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await SaveCartToLocalStorageAsync(memberId, cart);
            }

            return cart;
        }

        private async Task<CartDTO> RemoveFromDatabaseCartAsync(int memberId, int productId)
        {
            await _unitOfWork.Carts.RemoveItemFromCartAsync(memberId, productId);
            return await GetCartFromDatabaseAsync(memberId);
        }

        public async Task<bool> ClearCartAsync(int memberId, string role)
        {
            if (ShouldUseLocalStorage(role))
            {
                return await ClearLocalStorageCartAsync(memberId);
            }
            else
            {
                return await ClearDatabaseCartAsync(memberId);
            }
        }

        private async Task<bool> ClearLocalStorageCartAsync(int memberId)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", GetCartKey(memberId));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing localStorage cart: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ClearDatabaseCartAsync(int memberId)
        {
            try
            {
                return await _unitOfWork.Carts.ClearCartAsync(memberId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing database cart: {ex.Message}");
                return false;
            }
        }

        private async Task SaveCartToLocalStorageAsync(int memberId, CartDTO cart)
        {
            try
            {
                var cartJson = JsonSerializer.Serialize(cart);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GetCartKey(memberId), cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cart to localStorage: {ex.Message}");
            }
        }
    }
} 
