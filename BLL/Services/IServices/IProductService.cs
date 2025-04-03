using BLL.DTOs;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IProductService
    {
        Task<List<ProductSelectModel>> GetProductsAsync();
        Task<bool> CheckStockAvailabilityAsync(int productId, int quantity);
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int productId);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDto);
        Task<ProductDTO> UpdateProductAsync(UpdateProductDTO productDto);
        Task<bool> DeleteProductAsync(int productId);
        
        // Phương thức tìm kiếm sản phẩm
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string? productName, decimal? minPrice, decimal? maxPrice, string? categoryName);
        
        // Phương thức lấy sản phẩm theo trang
        Task<(IEnumerable<ProductDTO> Products, int TotalCount)> GetPagedProductsAsync(int pageNumber, int pageSize);
        
        // Phương thức giảm số lượng hàng trong kho khi đặt hàng
        Task<bool> DecreaseStockAsync(int productId, int quantity);
        
        // Phương thức giảm đồng thời số lượng hàng cho nhiều sản phẩm
        Task<bool> DecreaseStockForMultipleProductsAsync(Dictionary<int, int> productQuantities);
    }
}
