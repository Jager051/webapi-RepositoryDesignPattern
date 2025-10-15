using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting all categories
    /// </summary>
    public class GetAllCategoriesOrchestrator : IOrchestrator<object?, IEnumerable<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCategoriesOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<CategoryDto>>> ExecuteAsync(object? input)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetCategoriesWithProductsAsync();
                var categoryDtos = categories.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<CategoryDto>>.FailureResult($"Failed to get categories: {ex.Message}");
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

