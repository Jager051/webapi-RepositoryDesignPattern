using Microsoft.EntityFrameworkCore;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Infrastructure.Data;

namespace WebAPI.Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int? entityId = null)
        {
            var query = _dbSet.Where(a => a.EntityName == entityName);

            if (entityId.HasValue)
            {
                query = query.Where(a => a.EntityId == entityId.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50)
        {
            return await _dbSet
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityName, int entityId)
        {
            return await _dbSet
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}

