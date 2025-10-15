using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for searching products by name
    /// </summary>
    public class SearchProductsOrchestrator : IOrchestrator<string, IEnumerable<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchProductsOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<ProductDto>>> ExecuteAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return OrchestratorResult<IEnumerable<ProductDto>>.ValidationFailure(
                        new List<string> { "Search term cannot be empty" });
                }

                var products = await _unitOfWork.Products.SearchProductsByNameAsync(searchTerm);
                var productDtos = products.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<ProductDto>>.FailureResult($"Failed to search products: {ex.Message}");
            }
        }

        private static ProductDto MapToDto(Core.Entities.Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SKU = product.SKU,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}

