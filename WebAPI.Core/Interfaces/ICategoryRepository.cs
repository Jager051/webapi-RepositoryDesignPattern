using WebAPI.Core.Entities;

namespace WebAPI.Core.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Custom queries specific to Category
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesWithProductsAsync();
        Task<Category?> GetCategoryWithProductsAsync(int categoryId);
        Task<IEnumerable<Category>> SearchCategoriesByNameAsync(string searchTerm);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeCategoryId = null);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
    }
}

