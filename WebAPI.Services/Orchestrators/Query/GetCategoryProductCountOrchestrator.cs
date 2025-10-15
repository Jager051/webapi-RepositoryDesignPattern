using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Orchestrators.Query
{
    /// <summary>
    /// Orchestrator for getting product count by category
    /// </summary>
    public class GetCategoryProductCountOrchestrator : IOrchestrator<int, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryProductCountOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrchestratorResult<int>> ExecuteAsync(int categoryId)
        {
            try
            {
                var count = await _unitOfWork.Categories.GetProductCountByCategoryAsync(categoryId);
                return OrchestratorResult<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return OrchestratorResult<int>.FailureResult($"Failed to get product count: {ex.Message}");
            }
        }
    }
}

