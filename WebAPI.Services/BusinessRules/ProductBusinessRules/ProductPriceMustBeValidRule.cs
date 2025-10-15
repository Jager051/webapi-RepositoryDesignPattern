using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.ProductBusinessRules
{
    /// <summary>
    /// Validates that product price is within acceptable range
    /// </summary>
    public class ProductPriceMustBeValidRule : IBusinessRule<Product>
    {
        private const decimal MinPrice = 0.01m;
        private const decimal MaxPrice = 999999.99m;

        public Task<BusinessRuleResult> ValidateAsync(Product product)
        {
            if (product.Price < MinPrice)
            {
                return Task.FromResult(BusinessRuleResult.Failure(
                    $"Product price must be at least {MinPrice:C}", 
                    "PRODUCT_PRICE_TOO_LOW"));
            }

            if (product.Price > MaxPrice)
            {
                return Task.FromResult(BusinessRuleResult.Failure(
                    $"Product price cannot exceed {MaxPrice:C}", 
                    "PRODUCT_PRICE_TOO_HIGH"));
            }

            return Task.FromResult(BusinessRuleResult.Success());
        }
    }
}

