using WebAPI.Core.Entities;

namespace WebAPI.Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        // Custom queries specific to Product
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
        Task<IEnumerable<Product>> SearchProductsByNameAsync(string searchTerm);
        Task<Product?> GetProductBySkuAsync(string sku);
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
    }
}

