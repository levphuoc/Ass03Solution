using AutoMapper;
using BLL.DTOs;
using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IHubContext<CategoryHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository categoryRepository, IHubContext<CategoryHub> hubContext, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _hubContext = hubContext;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Categories>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            return await _categoryRepository.GetAllPagedAsync(pageNumber, pageSize);
        }

        public async Task<Categories> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task AddCategoryAsync(Categories category)
        {
            await _categoryRepository.AddAsync(category);
            var categoryDto = _mapper.Map<CategoryDTO>(category);
            await _hubContext.Clients.All.SendAsync("CategoryCreated", categoryDto);
        }

        public async Task UpdateCategoryAsync(Categories category)
        {
            await _categoryRepository.UpdateAsync(category);
            var categoryDto = _mapper.Map<CategoryDTO>(category);
            await _hubContext.Clients.All.SendAsync("CategoryUpdated", categoryDto);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            await _hubContext.Clients.All.SendAsync("CategoryDeleted", id);
        }
        
        public async Task<bool> CanDeleteCategoryAsync(int id)
        {
            var hasProducts = await _unitOfWork.Products.HasProductsByCategoryIdAsync(id);
            return !hasProducts;
        }
        
        // DTO Methods
        public async Task AddCategoryAsync(CreateCategoryDTO categoryDto)
        {
            var category = _mapper.Map<Categories>(categoryDto);
            await _categoryRepository.AddAsync(category);
            var createdCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);
            var categoryDto2 = _mapper.Map<CategoryDTO>(createdCategory);
            await _hubContext.Clients.All.SendAsync("CategoryCreated", categoryDto2);
        }

        public async Task UpdateCategoryAsync(UpdateCategoryDTO categoryDto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.CategoryId);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryDto.CategoryId} not found");
            }
            
            existingCategory.CategoryName = categoryDto.CategoryName;
            existingCategory.Description = categoryDto.Description;
            
            await _categoryRepository.UpdateAsync(existingCategory);
            var updatedCategoryDto = _mapper.Map<CategoryDTO>(existingCategory);
            await _hubContext.Clients.All.SendAsync("CategoryUpdated", updatedCategoryDto);
        }
    }
}
