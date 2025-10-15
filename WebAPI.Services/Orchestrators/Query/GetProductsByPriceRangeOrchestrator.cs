using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    public class PriceRangeQuery
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    /// <summary>
    /// Orchestrator for getting products by price range
    /// </summary>
    public class GetProductsByPriceRangeOrchestrator : IOrchestrator<PriceRangeQuery, IEnumerable<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductsByPriceRangeOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<IEnumerable<ProductDto>>> ExecuteAsync(PriceRangeQuery input)
        {
            try
            {
                if (input.MinPrice < 0 || input.MaxPrice < 0)
                {
                    return OrchestratorResult<IEnumerable<ProductDto>>.ValidationFailure(
                        new List<string> { "Prices cannot be negative" });
                }

                if (input.MinPrice > input.MaxPrice)
                {
                    return OrchestratorResult<IEnumerable<ProductDto>>.ValidationFailure(
                        new List<string> { "Min price cannot be greater than max price" });
                }

                var products = await _unitOfWork.Products.GetProductsByPriceRangeAsync(input.MinPrice, input.MaxPrice);
                var productDtos = products.Select(MapToDto);
                
                return OrchestratorResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<IEnumerable<ProductDto>>.FailureResult($"Failed to get products by price range: {ex.Message}");
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

