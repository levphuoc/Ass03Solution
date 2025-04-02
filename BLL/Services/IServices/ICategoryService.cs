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
    }
}
