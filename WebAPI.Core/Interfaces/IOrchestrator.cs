namespace WebAPI.Core.Interfaces
{
    /// <summary>
    /// Orchestrator for complex business operations
    /// </summary>
    public interface IOrchestrator<TInput, TOutput>
    {
        Task<OrchestratorResult<TOutput>> ExecuteAsync(TInput input);
    }

    /// <summary>
    /// Orchestrator execution result
    /// </summary>
    public class OrchestratorResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();

        public static OrchestratorResult<T> SuccessResult(T data)
            => new() { Success = true, Data = data };

        public static OrchestratorResult<T> FailureResult(string errorMessage)
            => new() { Success = false, ErrorMessage = errorMessage };

        public static OrchestratorResult<T> ValidationFailure(List<string> errors)
            => new() { Success = false, ValidationErrors = errors };
    }
}

