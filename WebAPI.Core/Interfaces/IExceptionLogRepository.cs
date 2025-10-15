using WebAPI.Core.Entities;

namespace WebAPI.Core.Interfaces
{
    public interface IExceptionLogRepository : IRepository<ExceptionLog>
    {
        /// <summary>
        /// Get exception logs by user
        /// </summary>
        Task<IEnumerable<ExceptionLog>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Get exception logs by severity
        /// </summary>
        Task<IEnumerable<ExceptionLog>> GetBySeverityAsync(string severity);

        /// <summary>
        /// Get exception logs within date range
        /// </summary>
        Task<IEnumerable<ExceptionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get recent exception logs
        /// </summary>
        Task<IEnumerable<ExceptionLog>> GetRecentExceptionsAsync(int count = 50);

        /// <summary>
        /// Get critical exceptions
        /// </summary>
        Task<IEnumerable<ExceptionLog>> GetCriticalExceptionsAsync();
    }
}

