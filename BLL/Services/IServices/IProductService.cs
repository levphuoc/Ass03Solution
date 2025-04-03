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
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int productId);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDto);
        Task<ProductDTO> UpdateProductAsync(UpdateProductDTO productDto);
        Task<bool> DeleteProductAsync(int productId);
    }
}
