using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.CategoryBusinessRules
{
    /// <summary>
    /// Validates that category can be deleted (has no products)
    /// </summary>
    public class CategoryCannotBeDeletedWithProductsRule : IBusinessRule<Category>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryCannotBeDeletedWithProductsRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessRuleResult> ValidateAsync(Category category)
        {
            var productCount = await _unitOfWork.Categories.GetProductCountByCategoryAsync(category.Id);
            
            if (productCount > 0)
            {
                return BusinessRuleResult.Failure(
                    $"Cannot delete category '{category.Name}' because it has {productCount} product(s)", 
                    "CATEGORY_HAS_PRODUCTS");
            }

            return BusinessRuleResult.Success();
        }
    }
}

