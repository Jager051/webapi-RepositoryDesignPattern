using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for searching categories by name
    /// </summary>
    public class SearchCategoriesOrchestrator : IOrchestrator<string, IEnumerable<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchCategoriesOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<CategoryDto>>> ExecuteAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return OrchestratorResult<IEnumerable<CategoryDto>>.ValidationFailure(
                        new List<string> { "Search term cannot be empty" });
                }

                var categories = await _unitOfWork.Categories.SearchCategoriesByNameAsync(searchTerm);
                var categoryDtos = categories.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<CategoryDto>>.FailureResult($"Failed to search categories: {ex.Message}");
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

