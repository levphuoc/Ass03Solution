using AutoMapper;
using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.UnitOfWork;
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
        private readonly IJSRuntime _jsRuntime;
        private readonly IProductService _productService;
        private const string CartKey = "estore_cart_";

        public CartService(IJSRuntime jsRuntime, IProductService productService)
        {
            _jsRuntime = jsRuntime;
            _productService = productService;
        }

        private string GetCartKey(int memberId) => $"{CartKey}{memberId}";

        public async Task<CartDTO> GetCartAsync(int memberId)
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
                Console.WriteLine($"Error fetching cart: {ex.Message}");
                return new CartDTO();
            }
        }

        public async Task<CartDTO> AddToCartAsync(int memberId, int productId, int quantity = 1)
        {
            if (quantity <= 0)
                quantity = 1;

            var cart = await GetCartAsync(memberId);
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return cart;

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
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

            await SaveCartAsync(memberId, cart);
            return cart;
        }

        public async Task<CartDTO> UpdateCartItemAsync(int memberId, int productId, int quantity)
        {
            var cart = await GetCartAsync(memberId);
            
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

            await SaveCartAsync(memberId, cart);
            return cart;
        }

        public async Task<CartDTO> RemoveFromCartAsync(int memberId, int productId)
        {
            var cart = await GetCartAsync(memberId);
            
            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await SaveCartAsync(memberId, cart);
            }

            return cart;
        }

        public async Task<bool> ClearCartAsync(int memberId)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", GetCartKey(memberId));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cart: {ex.Message}");
                return false;
            }
        }

        private async Task SaveCartAsync(int memberId, CartDTO cart)
        {
            try
            {
                var cartJson = JsonSerializer.Serialize(cart);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GetCartKey(memberId), cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cart: {ex.Message}");
            }
        }
    }
} 