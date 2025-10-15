using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.ProductBusinessRules
{
    /// <summary>
    /// Validates that product SKU is unique
    /// </summary>
    public class ProductSkuMustBeUniqueRule : IBusinessRule<Product>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductSkuMustBeUniqueRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessRuleResult> ValidateAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                return BusinessRuleResult.Failure(
                    "Product SKU cannot be empty", 
                    "PRODUCT_SKU_EMPTY");
            }

            var isUnique = await _unitOfWork.Products.IsSkuUniqueAsync(product.SKU, product.Id);
            
            if (!isUnique)
            {
                return BusinessRuleResult.Failure(
                    $"Product with SKU '{product.SKU}' already exists", 
                    "PRODUCT_SKU_DUPLICATE");
            }

            return BusinessRuleResult.Success();
        }
    }
}

