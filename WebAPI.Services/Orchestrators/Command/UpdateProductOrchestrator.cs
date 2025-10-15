using WebAPI.Core.DTOs;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Services.BusinessRules.ProductBusinessRules;

namespace WebAPI.Services.Orchestrators.Command
{
    /// <summary>
    /// Orchestrates product update with business rules
    /// </summary>
    public class UpdateProductOrchestrator : IOrchestrator<(int Id, UpdateProductDto Dto), ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<ProductDto>> ExecuteAsync((int Id, UpdateProductDto Dto) input)
        {
            // 1. Get existing product
            var product = await _unitOfWork.Products.GetByIdAsync(input.Id);
            if (product == null)
            {
                return OrchestratorResult<ProductDto>.FailureResult("Product not found");
            }

            // 2. Update product properties
            product.Name = input.Dto.Name;
            product.Description = input.Dto.Description;
            product.Price = input.Dto.Price;
            product.SKU = input.Dto.SKU;
            product.StockQuantity = input.Dto.StockQuantity;
            product.CategoryId = input.Dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            // 3. Create business rules (NEW ile oluşturuluyor)
            var rules = new List<IBusinessRule<Product>>
            {
                new ProductSkuMustBeUniqueRule(_unitOfWork),
                new ProductMustHaveValidCategoryRule(_unitOfWork),
                new ProductPriceMustBeValidRule(),
                new ProductStockMustBeValidRule()
            };

            // 4. Validate all business rules
            var validationErrors = new List<string>();
            foreach (var rule in rules)
            {
                var result = await rule.ValidateAsync(product);
                if (!result.IsValid)
                {
                    validationErrors.Add(result.ErrorMessage);
                }
            }

            if (validationErrors.Any())
            {
                return OrchestratorResult<ProductDto>.ValidationFailure(validationErrors);
            }

            // 5. Execute transaction
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Load category for response
                var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

                await _unitOfWork.CommitTransactionAsync();

                // 6. Return success result
                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    SKU = product.SKU,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return OrchestratorResult<ProductDto>.SuccessResult(productDto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OrchestratorResult<ProductDto>.FailureResult($"Failed to update product: {ex.Message}");
            }
        }
    }
}

