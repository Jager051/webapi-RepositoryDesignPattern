using WebAPI.Core.Entities;

namespace WebAPI.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        /// <summary>
        /// Get audit logs by user
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Get audit logs by action
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);

        /// <summary>
        /// Get audit logs by entity
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int? entityId = null);

        /// <summary>
        /// Get audit logs within date range
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get recent audit logs
        /// </summary>
        Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50);

        /// <summary>
        /// Get entity change history
        /// </summary>
        Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityName, int entityId);
    }
}

