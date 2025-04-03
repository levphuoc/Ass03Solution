using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Categories>> GetAllPagedAsync(int pageNumber, int pageSize);
        Task<Categories> GetByIdAsync(int id);
        Task AddAsync(Categories category);
        Task UpdateAsync(Categories category);
        Task DeleteAsync(int id);
        Task<IEnumerable<Categories>> GetAllAsync();
    }
}
