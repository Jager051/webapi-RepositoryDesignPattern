using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.BusinessRules.ProductBusinessRules
{
    /// <summary>
    /// Validates that product stock quantity is valid
    /// </summary>
    public class ProductStockMustBeValidRule : IBusinessRule<Product>
    {
        private const int MinStock = 0;
        private const int MaxStock = 100000;

        public Task<BusinessRuleResult> ValidateAsync(Product product)
        {
            if (product.StockQuantity < MinStock)
            {
                return Task.FromResult(BusinessRuleResult.Failure(
                    $"Stock quantity cannot be negative", 
                    "PRODUCT_STOCK_NEGATIVE"));
            }

            if (product.StockQuantity > MaxStock)
            {
                return Task.FromResult(BusinessRuleResult.Failure(
                    $"Stock quantity cannot exceed {MaxStock}", 
                    "PRODUCT_STOCK_TOO_HIGH"));
            }

            return Task.FromResult(BusinessRuleResult.Success());
        }
    }
}

