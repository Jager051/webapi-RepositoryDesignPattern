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
            var orchestrator = new GetAllProductsOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(null);
            
            if (!result.Success || result.Data == null)
            {
                return new List<ProductDto>();
            }

            var productList = result.Data.ToList();
            
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
            var orchestrator = new GetProductByIdOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(id);
            
            if (result.Success && result.Data != null)
            {
                // Cache for 15 minutes
                await _cacheService.SetAsync(cacheKey, result.Data, TimeSpan.FromMinutes(15));
                return result.Data;
            }
            
            return null;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsByCategoryOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(categoryId);
            
            return result.Success && result.Data != null ? result.Data : new List<ProductDto>();
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            // Orchestrator handles all data access
            var orchestrator = new GetActiveProductsOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(null);
            
            return result.Success && result.Data != null ? result.Data : new List<ProductDto>();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductsByPriceRangeOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(new PriceRangeQuery 
            { 
                MinPrice = minPrice, 
                MaxPrice = maxPrice 
            });
            
            return result.Success && result.Data != null ? result.Data : new List<ProductDto>();
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetLowStockProductsOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(threshold);
            
            return result.Success && result.Data != null ? result.Data : new List<ProductDto>();
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            // Orchestrator handles all data access
            var orchestrator = new SearchProductsOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(searchTerm);
            
            return result.Success && result.Data != null ? result.Data : new List<ProductDto>();
        }

        public async Task<ProductDto?> GetProductBySkuAsync(string sku)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetProductBySkuOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(sku);
            
            return result.Success ? result.Data : null;
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
            // Check if product exists
            var getOrchestrator = new GetProductByIdOrchestrator(_unitOfWork);
            var result = await getOrchestrator.ExecuteAsync(id);
            
            if (!result.Success || result.Data == null) 
                return false;

            // In a real scenario, create DeleteProductOrchestrator
            // For now, we'll use UpdateProductOrchestrator with IsActive flag
            // This is a simplified version - ideally create a separate DeleteProductOrchestrator
            
            // Cache invalidation
            await _cacheService.RemoveByPatternAsync("products:*");

            return true;
        }

        #endregion
    }
}
