using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.UnitOfWork;
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

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
