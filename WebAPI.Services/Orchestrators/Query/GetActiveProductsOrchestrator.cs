using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting active products
    /// </summary>
    public class GetActiveProductsOrchestrator : IOrchestrator<object?, IEnumerable<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetActiveProductsOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<ProductDto>>> ExecuteAsync(object? input)
        {
            try
            {
                var products = await _unitOfWork.Products.GetActiveProductsAsync();
                var productDtos = products.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<ProductDto>>.FailureResult($"Failed to get active products: {ex.Message}");
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

