using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for querying categories
    /// </summary>
    public class GetCategoriesOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoriesOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Categories.GetCategoriesWithProductsAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(id);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
            return categories.Select(MapToDto);
        }

        public async Task<IEnumerable<CategoryDto>> SearchAsync(string searchTerm)
        {
            var categories = await _unitOfWork.Categories.SearchCategoriesByNameAsync(searchTerm);
            return categories.Select(MapToDto);
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _unitOfWork.Categories.GetProductCountByCategoryAsync(categoryId);
        }

        private static CategoryDto MapToDto(Core.Entities.Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ProductCount = category.Products?.Count ?? 0,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}

