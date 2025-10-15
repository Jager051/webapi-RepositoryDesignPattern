using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.CategoryBusinessRules
{
    /// <summary>
    /// Validates that category name is unique
    /// </summary>
    public class CategoryNameMustBeUniqueRule : IBusinessRule<Category>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryNameMustBeUniqueRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessRuleResult> ValidateAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BusinessRuleResult.Failure(
                    "Category name cannot be empty", 
                    "CATEGORY_NAME_EMPTY");
            }

            var isUnique = await _unitOfWork.Categories.IsCategoryNameUniqueAsync(
                category.Name, 
                category.Id);
            
            if (!isUnique)
            {
                return BusinessRuleResult.Failure(
                    $"Category with name '{category.Name}' already exists", 
                    "CATEGORY_NAME_DUPLICATE");
            }

            return BusinessRuleResult.Success();
        }
    }
}

