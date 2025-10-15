using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting a product by SKU
    /// </summary>
    public class GetProductBySkuOrchestrator : IOrchestrator<string, ProductDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductBySkuOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<ProductDto?>> ExecuteAsync(string sku)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sku))
                {
                    return OrchestratorResult<ProductDto?>.ValidationFailure(
                        new List<string> { "SKU cannot be empty" });
                }

                var product = await _unitOfWork.Products.GetProductBySkuAsync(sku);
                
                if (product == null)
                {
                    return OrchestratorResult<ProductDto?>.FailureResult($"Product with SKU '{sku}' not found");
                }

                var productDto = MapToDto(product);
                return OrchestratorResult<ProductDto?>.SuccessResult(productDto);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<ProductDto?>.FailureResult($"Failed to get product by SKU: {ex.Message}");
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

