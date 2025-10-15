using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Services.BusinessRules.CategoryBusinessRules;

namespace WebAPI.Services.Orchestrators.Command
{
    /// <summary>
    /// Orchestrates category deletion with business rules
    /// </summary>
    public class DeleteCategoryOrchestrator : IOrchestrator<int, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<bool>> ExecuteAsync(int categoryId)
        {
            // 1. Get category
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                return OrchestratorResult<bool>.FailureResult("Category not found");
            }

            // 2. Create business rule (NEW ile oluşturuluyor)
            var rule = new CategoryCannotBeDeletedWithProductsRule(_unitOfWork);

            // 3. Validate
            var validationResult = await rule.ValidateAsync(category);
            if (!validationResult.IsValid)
            {
                return OrchestratorResult<bool>.ValidationFailure(
                    new List<string> { validationResult.ErrorMessage });
            }

            // 4. Execute soft delete
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return OrchestratorResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OrchestratorResult<bool>.FailureResult($"Failed to delete category: {ex.Message}");
            }
        }
    }
}

