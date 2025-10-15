using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting a category by ID
    /// </summary>
    public class GetCategoryByIdOrchestrator : IOrchestrator<int, CategoryDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryByIdOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<CategoryDto?>> ExecuteAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(id);
                
                if (category == null)
                {
                    return OrchestratorResult<CategoryDto?>.FailureResult($"Category with ID {id} not found");
                }

                var categoryDto = MapToDto(category);
                return OrchestratorResult<CategoryDto?>.SuccessResult(categoryDto);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<CategoryDto?>.FailureResult($"Failed to get category: {ex.Message}");
            }
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

