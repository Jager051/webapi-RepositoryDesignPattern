namespace WebAPI.Core.Interfaces
{
    /// <summary>
    /// Base interface for all business rules
    /// </summary>
    public interface IBusinessRule<T>
    {
        Task<BusinessRuleResult> ValidateAsync(T entity);
    }

    /// <summary>
    /// Business rule validation result
    /// </summary>
    public class BusinessRuleResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;

        public static BusinessRuleResult Success() => new() { IsValid = true };
        
        public static BusinessRuleResult Failure(string errorMessage, string errorCode = "")
            => new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}

