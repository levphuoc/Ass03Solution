using AutoMapper;
using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        // GET: api/Category?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
            return Ok(categoryDtos);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = _mapper.Map<CategoryDTO>(category);
            return Ok(categoryDto);
        }

        // POST: api/Category
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<ActionResult<CategoryDTO>> CreateCategory(CreateCategoryDTO categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = _mapper.Map<Categories>(categoryDto);
                await _categoryService.AddCategoryAsync(category);

                var createdCategoryDto = _mapper.Map<CategoryDTO>(category);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDto.CategoryId }, createdCategoryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDTO categoryDto)
        {
            try
            {
                if (id != categoryDto.CategoryId)
                {
                    return BadRequest("Category ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                var category = _mapper.Map<Categories>(categoryDto);
                await _categoryService.UpdateCategoryAsync(category);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                
                // Kiểm tra xem có thể xóa category không
                var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
                if (!canDelete)
                {
                    return BadRequest("Cannot delete this category because there are products associated with it.");
                }

                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}