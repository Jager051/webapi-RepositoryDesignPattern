using WebAPI.Core.DTOs;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Services.BusinessRules.ProductBusinessRules;

namespace WebAPI.Services.Orchestrators.Command
{
    /// <summary>
    /// Orchestrates complex product operations with business rules
    /// </summary>
    public class ProductOrchestrator : IOrchestrator<CreateProductDto, ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<ProductDto>> ExecuteAsync(CreateProductDto input)
        {
            // 1. Map DTO to Entity
            var product = new Product
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                SKU = input.SKU,
                StockQuantity = input.StockQuantity,
                CategoryId = input.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Create business rules (NEW ile oluşturuluyor - boşuna DI kullanmıyoruz)
            var rules = new List<IBusinessRule<Product>>
            {
                new ProductSkuMustBeUniqueRule(_unitOfWork),
                new ProductMustHaveValidCategoryRule(_unitOfWork),
                new ProductPriceMustBeValidRule(),
                new ProductStockMustBeValidRule()
            };

            // 3. Validate all business rules
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

            // 4. Execute transaction
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Save product
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Load category for response
                var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

                await _unitOfWork.CommitTransactionAsync();

                // 5. Return success result
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
                return OrchestratorResult<ProductDto>.FailureResult($"Failed to create product: {ex.Message}");
            }
        }
    }
}

