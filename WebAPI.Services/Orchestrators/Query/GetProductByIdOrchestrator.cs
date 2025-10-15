using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting a product by ID
    /// </summary>
    public class GetProductByIdOrchestrator : IOrchestrator<int, ProductDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<ProductDto?>> ExecuteAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                
                if (product == null)
                {
                    return OrchestratorResult<ProductDto?>.FailureResult($"Product with ID {id} not found");
                }

                var productDto = MapToDto(product);
                return OrchestratorResult<ProductDto?>.SuccessResult(productDto);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<ProductDto?>.FailureResult($"Failed to get product: {ex.Message}");
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

