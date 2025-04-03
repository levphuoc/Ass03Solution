using BLL.DTOs;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<Categories>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<Categories> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(Categories category);
        Task UpdateCategoryAsync(Categories category);
        Task DeleteCategoryAsync(int id);
        
        // Kiểm tra xem category có thể xóa không
        Task<bool> CanDeleteCategoryAsync(int id);
        
        // New methods for DTOs
        Task AddCategoryAsync(CreateCategoryDTO categoryDto);
        Task UpdateCategoryAsync(UpdateCategoryDTO categoryDto);
    }
}
