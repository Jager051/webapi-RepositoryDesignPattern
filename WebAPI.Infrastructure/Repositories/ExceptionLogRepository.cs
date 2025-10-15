using Microsoft.EntityFrameworkCore;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;
using WebAPI.Infrastructure.Data;

namespace WebAPI.Infrastructure.Repositories
{
    public class ExceptionLogRepository : GenericRepository<ExceptionLog>, IExceptionLogRepository
    {
        public ExceptionLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExceptionLog>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExceptionLog>> GetBySeverityAsync(string severity)
        {
            return await _dbSet
                .Where(e => e.Severity == severity)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExceptionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExceptionLog>> GetRecentExceptionsAsync(int count = 50)
        {
            return await _dbSet
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExceptionLog>> GetCriticalExceptionsAsync()
        {
            return await _dbSet
                .Where(e => e.Severity == "Critical")
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}

