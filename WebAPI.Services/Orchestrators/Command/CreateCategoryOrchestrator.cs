using WebAPI.Core.DTOs;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Services.BusinessRules.CategoryBusinessRules;

namespace WebAPI.Services.Orchestrators.Command
{
    /// <summary>
    /// Orchestrator for creating a category
    /// </summary>
    public class CreateCategoryOrchestrator : IOrchestrator<CreateCategoryDto, CategoryDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<CategoryDto>> ExecuteAsync(CreateCategoryDto input)
        {
            // 1. Map DTO to Entity
            var category = new Category
            {
                Name = input.Name,
                Description = input.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Create business rules
            var rules = new List<IBusinessRule<Category>>
            {
                new CategoryNameMustBeUniqueRule(_unitOfWork)
            };

            // 3. Validate all business rules
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

            // 4. Execute transaction
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // 5. Return success result
                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    ProductCount = 0,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return OrchestratorResult<CategoryDto>.SuccessResult(categoryDto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OrchestratorResult<CategoryDto>.FailureResult($"Failed to create category: {ex.Message}");
            }
        }
    }
}

