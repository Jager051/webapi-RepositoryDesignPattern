using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;
using WebAPI.Services.Orchestrators.Command;
using WebAPI.Services.Orchestrators.Query;

namespace WebAPI.Services.Services
{
    /// <summary>
    /// Category service - coordinates orchestrators
    /// NO direct UnitOfWork access - all data access through orchestrators
    /// </summary>
    public class CategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Query Operations (Read)

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            // Orchestrator handles all data access
            var orchestrator = new GetAllCategoriesOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(null);
            
            return result.Success && result.Data != null ? result.Data : new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetCategoryByIdOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(id);
            
            return result.Success ? result.Data : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            // Orchestrator handles all data access
            var orchestrator = new GetActiveCategoriesOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(null);
            
            return result.Success && result.Data != null ? result.Data : new List<CategoryDto>();
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithProductsAsync()
        {
            // Orchestrator handles all data access (GetAllCategories returns categories with products)
            var orchestrator = new GetAllCategoriesOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(null);
            
            return result.Success && result.Data != null ? result.Data : new List<CategoryDto>();
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            // Orchestrator handles all data access
            var orchestrator = new GetCategoryProductCountOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(categoryId);
            
            return result.Success ? result.Data : 0;
        }

        public async Task<IEnumerable<CategoryDto>> SearchCategoriesAsync(string searchTerm)
        {
            // Orchestrator handles all data access
            var orchestrator = new SearchCategoriesOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(searchTerm);
            
            return result.Success && result.Data != null ? result.Data : new List<CategoryDto>();
        }

        #endregion

        #region Command Operations (Write)

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            // Orchestrator handles business rules, validation, and data access
            var orchestrator = new CreateCategoryOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(createCategoryDto);

            if (!result.Success)
            {
                var errorMessage = result.ValidationErrors.Any() 
                    ? string.Join(", ", result.ValidationErrors)
                    : result.ErrorMessage;
                throw new InvalidOperationException(errorMessage);
            }

            return result.Data!;
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            // Orchestrator handles business rules, validation, and data access
            var orchestrator = new UpdateCategoryOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync((id, updateCategoryDto));

            if (!result.Success)
            {
                if (result.ErrorMessage.Contains("not found"))
                    return null;

                var errorMessage = result.ValidationErrors.Any() 
                    ? string.Join(", ", result.ValidationErrors)
                    : result.ErrorMessage;
                throw new InvalidOperationException(errorMessage);
            }

            return result.Data;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            // Orchestrator handles business rules, validation, and data access
            var orchestrator = new DeleteCategoryOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(id);

            if (!result.Success)
            {
                // Validation errors veya not found
                return false;
            }

            return result.Data;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            // Simple check through query orchestrator
            var orchestrator = new GetCategoryByIdOrchestrator(_unitOfWork);
            var result = await orchestrator.ExecuteAsync(id);
            
            return result.Success && result.Data != null;
        }

        #endregion
    }
}
