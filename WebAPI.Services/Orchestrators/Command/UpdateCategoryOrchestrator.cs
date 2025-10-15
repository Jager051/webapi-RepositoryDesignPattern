using WebAPI.Core.DTOs;
using WebAPI.Core.Interfaces;
using WebAPI.Services.BusinessRules.CategoryBusinessRules;

namespace WebAPI.Services.Orchestrators.Command
{
    /// <summary>
    /// Orchestrator for updating a category
    /// </summary>
    public class UpdateCategoryOrchestrator : IOrchestrator<(int Id, UpdateCategoryDto Dto), CategoryDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<CategoryDto>> ExecuteAsync((int Id, UpdateCategoryDto Dto) input)
        {
            // 1. Get existing category
            var category = await _unitOfWork.Categories.GetByIdAsync(input.Id);
            if (category == null)
            {
                return OrchestratorResult<CategoryDto>.FailureResult("Category not found");
            }

            // 2. Update properties
            category.Name = input.Dto.Name;
            category.Description = input.Dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            // 3. Create business rules
            var rules = new List<IBusinessRule<Core.Entities.Category>>
            {
                new CategoryNameMustBeUniqueRule(_unitOfWork)
            };

            // 4. Validate
            var validationErrors = new List<string>();
            foreach (var rule in rules)
            {
                var result = await rule.ValidateAsync(category);
                if (!result.IsValid)
                {
                    validationErrors.Add(result.ErrorMessage);
                }
            }

            if (validationErrors.Any())
            {
                return OrchestratorResult<CategoryDto>.ValidationFailure(validationErrors);
            }

            // 5. Execute transaction
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    ProductCount = category.Products?.Count ?? 0,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return OrchestratorResult<CategoryDto>.SuccessResult(categoryDto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OrchestratorResult<CategoryDto>.FailureResult($"Failed to update category: {ex.Message}");
            }
        }
    }
}

