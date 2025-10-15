using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.ProductBusinessRules
{
    /// <summary>
    /// Validates that product category exists and is active
    /// </summary>
    public class ProductMustHaveValidCategoryRule : IBusinessRule<Product>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductMustHaveValidCategoryRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessRuleResult> ValidateAsync(Product product)
        {
            if (product.CategoryId <= 0)
            {
                return BusinessRuleResult.Failure(
                    "Product must have a valid category", 
                    "PRODUCT_CATEGORY_INVALID");
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
            
            if (category == null)
            {
                return BusinessRuleResult.Failure(
                    $"Category with ID {product.CategoryId} does not exist", 
                    "PRODUCT_CATEGORY_NOT_FOUND");
            }

            if (!category.IsActive)
            {
                return BusinessRuleResult.Failure(
                    $"Category '{category.Name}' is not active", 
                    "PRODUCT_CATEGORY_INACTIVE");
            }

            return BusinessRuleResult.Success();
        }
    }
}

