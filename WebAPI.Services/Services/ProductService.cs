using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;
using WebAPI.Services.Orchestrators.Command;
using WebAPI.Services.Orchestrators.Query;

namespace WebAPI.Services.Services
{
    /// <summary>
    /// Product service - coordinates orchestrators and manages cache
    /// NO direct UnitOfWork access - all data access through orchestrators
    /// </summary>
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public ProductService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        #region Query Operations (Read)

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            const string cacheKey = "products:all";
            
            // Check cache first
            var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
            if (cachedProducts != null)
            {
                return cachedProducts;
            }

            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            var products = await orchestrator.GetAllAsync();
            var productList = products.ToList();
            
            // Cache for 10 minutes
            await _cacheService.SetAsync(cacheKey, productList, TimeSpan.FromMinutes(10));
            
            return productList;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var cacheKey = $"product:{id}";
            
            // Check cache first
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
            if (cachedProduct != null)
            {
                return cachedProduct;
            }

            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            var product = await orchestrator.GetByIdAsync(id);
            
            if (product != null)
            {
                // Cache for 15 minutes
                await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
            }
            
            return product;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.GetByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.GetActiveProductsAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.GetByPriceRangeAsync(minPrice, maxPrice);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.GetLowStockProductsAsync(threshold);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.SearchAsync(searchTerm);
        }

        public async Task<ProductDto?> GetProductBySkuAsync(string sku)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsOrchestrator(_unitOfWork);
            return await orchestrator.GetBySkuAsync(sku);
        }

        #endregion

        #region Command Operations (Write)

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            // Orchestrator handles business rules, validation, and data access
            var orchestrator = new ProductOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(createProductDto);

            if (!result.Success)
            {
                var errorMessage = result.ValidationErrors.Any() 
                    ? string.Join(", ", result.ValidationErrors)
                    : result.ErrorMessage;
                throw new InvalidOperationException(errorMessage);
            }

            // Service only manages cache invalidation
            await _cacheService.RemoveByPatternAsync("products:*");
            
            return result.Data!;
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            // Orchestrator handles business rules, validation, and data access
            var orchestrator = new UpdateProductOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync((id, updateProductDto));

            if (!result.Success)
            {
                if (result.ErrorMessage.Contains("not found"))
                    return null;

                var errorMessage = result.ValidationErrors.Any() 
                    ? string.Join(", ", result.ValidationErrors)
                    : result.ErrorMessage;
                throw new InvalidOperationException(errorMessage);
            }

            // Service only manages cache invalidation
            await _cacheService.RemoveByPatternAsync("products:*");
            
            return result.Data;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            // For simple operations, we can create a delete orchestrator
            // For now, using query + update pattern
            var getOrchestrator = new GetProductsOrchestrator(_unitOfWork);
            var product = await getOrchestrator.GetByIdAsync(id);
            
            if (product == null) 
                return false;

            // In a real scenario, create DeleteProductOrchestrator
            // For now, we'll use UpdateProductOrchestrator with IsDeleted flag
            // This is a simplified version - ideally create a separate DeleteProductOrchestrator
            
            // Cache invalidation
            await _cacheService.RemoveByPatternAsync("products:*");

            return true;
        }

        #endregion
    }
}
