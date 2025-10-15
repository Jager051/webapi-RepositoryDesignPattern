using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting active categories
    /// </summary>
    public class GetActiveCategoriesOrchestrator : IOrchestrator<object?, IEnumerable<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetActiveCategoriesOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<CategoryDto>>> ExecuteAsync(object? input)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
                var categoryDtos = categories.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<CategoryDto>>.FailureResult($"Failed to get active categories: {ex.Message}");
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

