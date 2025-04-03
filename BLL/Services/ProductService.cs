using BLL.DTOs;
﻿using AutoMapper;
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHubContext<ProductHub> _productHub;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IHubContext<ProductHub> productHub, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _productHub = productHub;
            _mapper = mapper;
        }

        public async Task<List<ProductSelectModel>> GetProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();

            return products.Select(p => new ProductSelectModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice

            }).ToList();
        }
        public async Task<bool> CheckStockAvailabilityAsync(int productId, int quantity)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            return product != null && product.UnitsInStock >= quantity;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            // Lấy tất cả sản phẩm từ repository
            var products = await _unitOfWork.Products.GetAllAsync();
            
            // Lấy tất cả danh mục
            var categories = await _unitOfWork.Categories.GetAllAsync();
            
            // Map từ Product sang ProductDTO và thêm CategoryName
            var productDTOs = products.Select(p => {
                var dto = _mapper.Map<ProductDTO>(p);
                dto.CategoryName = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId)?.CategoryName ?? "Unknown";
                return dto;
            });
            
            return productDTOs;
        }

        public async Task<ProductDTO> GetProductByIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            
            if (product != null)
            {
                var dto = _mapper.Map<ProductDTO>(product);
                
                // Lấy thông tin Category
                var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
                if (category != null)
                {
                    dto.CategoryName = category.CategoryName;
                }
                
                return dto;
            }
            
            return null;
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            var createdProductDto = _mapper.Map<ProductDTO>(product);
            
            // Lấy thông tin Category
            var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
            if (category != null)
            {
                createdProductDto.CategoryName = category.CategoryName;
            }
            
            // Send realtime notification
            await _productHub.Clients.All.SendAsync("ProductCreated", createdProductDto);

            return createdProductDto;
        }

        public async Task<ProductDTO> UpdateProductAsync(UpdateProductDTO productDto)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                return null;

            _mapper.Map(productDto, existingProduct);
            
            await _unitOfWork.Products.UpdateAsync(existingProduct);
            await _unitOfWork.SaveChangesAsync();
            
            var updatedProductDto = _mapper.Map<ProductDTO>(existingProduct);
            
            // Lấy thông tin Category
            var category = await _unitOfWork.Categories.GetByIdAsync(existingProduct.CategoryId);
            if (category != null)
            {
                updatedProductDto.CategoryName = category.CategoryName;
            }
            
            // Send realtime notification
            await _productHub.Clients.All.SendAsync("ProductUpdated", updatedProductDto);

            return updatedProductDto;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return false;

            await _unitOfWork.Products.DeleteAsync(productId);
            await _unitOfWork.SaveChangesAsync();
            
            // Send realtime notification
            await _productHub.Clients.All.SendAsync("ProductDeleted", productId);

            return true;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string? productName, decimal? minPrice, decimal? maxPrice, string? categoryName)
        {
            // Lấy tất cả sản phẩm
            var products = await _unitOfWork.Products.GetAllAsync();
            var categories = await _unitOfWork.Categories.GetAllAsync();
            
            // Map sang DTO và thêm CategoryName
            var productDTOs = products.Select(p => {
                var dto = _mapper.Map<ProductDTO>(p);
                dto.CategoryName = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId)?.CategoryName ?? "Unknown";
                return dto;
            }).ToList();
            
            // Lọc theo các điều kiện
            var query = productDTOs.AsQueryable();
            
            // Lọc theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(productName))
            {
                query = query.Where(p => p.ProductName.Contains(productName, StringComparison.OrdinalIgnoreCase));
            }
            
            // Lọc theo giá tối thiểu
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice >= minPrice.Value);
            }
            
            // Lọc theo giá tối đa
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice <= maxPrice.Value);
            }
            
            // Lọc theo tên danh mục
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(p => p.CategoryName != null && 
                    p.CategoryName.Contains(categoryName, StringComparison.OrdinalIgnoreCase));
            }
            
            return query.ToList();
        }

        public async Task<(IEnumerable<ProductDTO> Products, int TotalCount)> GetPagedProductsAsync(int pageNumber, int pageSize)
        {
            // Sử dụng repository để lấy dữ liệu đã phân trang
            var (pagedProducts, totalCount) = await _unitOfWork.Products.GetPagedAsync(
                pageNumber,
                pageSize
            );
            
            // Lấy tất cả danh mục
            var categories = await _unitOfWork.Categories.GetAllAsync();
            
            // Map từ Product sang ProductDTO và thêm CategoryName
            var productDTOs = pagedProducts.Select(p => {
                var dto = _mapper.Map<ProductDTO>(p);
                dto.CategoryName = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId)?.CategoryName ?? "Unknown";
                return dto;
            }).ToList();
            
            return (productDTOs, totalCount);
        }
        
        public async Task<bool> DecreaseStockAsync(int productId, int quantity)
        {
            try
            {
                // Get the product
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {productId} not found when decreasing stock");
                    return false;
                }
                
                // Check if there's enough stock
                if (product.UnitsInStock < quantity)
                {
                    Console.WriteLine($"Not enough stock for product {productId}. Available: {product.UnitsInStock}, Requested: {quantity}");
                    return false;
                }
                
                // Decrease the stock
                product.UnitsInStock -= quantity;
                
                // Update the product
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
                
                // Notify clients about the stock update
                var updatedProductDto = _mapper.Map<ProductDTO>(product);
                var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
                if (category != null)
                {
                    updatedProductDto.CategoryName = category.CategoryName;
                }
                await _productHub.Clients.All.SendAsync("ProductUpdated", updatedProductDto);
                
                Console.WriteLine($"Stock decreased for product {productId}. New stock: {product.UnitsInStock}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decreasing stock for product {productId}: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> DecreaseStockForMultipleProductsAsync(Dictionary<int, int> productQuantities)
        {
            if (productQuantities == null || !productQuantities.Any())
                return true;  // Nothing to process
                
            try
            {
                // Check all products first to make sure we have enough stock
                foreach (var item in productQuantities)
                {
                    var productId = item.Key;
                    var quantity = item.Value;
                    
                    var product = await _unitOfWork.Products.GetByIdAsync(productId);
                    if (product == null)
                    {
                        Console.WriteLine($"Product with ID {productId} not found when checking stock");
                        return false;
                    }
                    
                    if (product.UnitsInStock < quantity)
                    {
                        Console.WriteLine($"Not enough stock for product {productId}. Available: {product.UnitsInStock}, Requested: {quantity}");
                        return false;
                    }
                }
                
                // If all checks pass, decrease stock for all products
                foreach (var item in productQuantities)
                {
                    var productId = item.Key;
                    var quantity = item.Value;
                    
                    var product = await _unitOfWork.Products.GetByIdAsync(productId);
                    product.UnitsInStock -= quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                    
                    Console.WriteLine($"Stock decreased for product {productId}. New stock: {product.UnitsInStock}");
                }
                
                // Save all changes in a single transaction
                await _unitOfWork.SaveChangesAsync();
                
                // Notify clients about updates
                await _productHub.Clients.All.SendAsync("ProductsUpdated");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decreasing stock for multiple products: {ex.Message}");
                return false;
            }
        }
    }
}
