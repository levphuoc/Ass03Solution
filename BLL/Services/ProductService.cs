using AutoMapper;
using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
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

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductByIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            var createdProductDto = _mapper.Map<ProductDTO>(product);
            
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
    }
}
