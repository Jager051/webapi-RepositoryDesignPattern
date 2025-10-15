using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting products by category
    /// </summary>
    public class GetProductsByCategoryOrchestrator : IOrchestrator<int, IEnumerable<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductsByCategoryOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<ProductDto>>> ExecuteAsync(int categoryId)
        {
            try
            {
                var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
                var productDtos = products.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<ProductDto>>.FailureResult($"Failed to get products by category: {ex.Message}");
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

