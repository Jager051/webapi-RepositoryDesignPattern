using Microsoft.AspNetCore.Http;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Services
{
    /// <summary>
    /// Service for managing exception logs
    /// </summary>
    public class ExceptionLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExceptionLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        public async Task LogExceptionAsync(Exception exception, int? userId = null, string severity = "Error")
        {
            var exceptionLog = CreateExceptionLog(exception, userId, severity);
            await SaveExceptionLogAsync(exceptionLog);
        }

        /// <summary>
        /// Log an exception with custom context
        /// </summary>
        public async Task LogExceptionAsync(Exception exception, HttpContext? httpContext, int? userId = null, string severity = "Error")
        {
            var exceptionLog = CreateExceptionLog(exception, userId, severity, httpContext);
            await SaveExceptionLogAsync(exceptionLog);
        }

        /// <summary>
        /// Log a critical exception
        /// </summary>
        public async Task LogCriticalExceptionAsync(Exception exception, int? userId = null)
        {
            await LogExceptionAsync(exception, userId, "Critical");
        }

        /// <summary>
        /// Log a warning exception
        /// </summary>
        public async Task LogWarningExceptionAsync(Exception exception, int? userId = null)
        {
            await LogExceptionAsync(exception, userId, "Warning");
        }

        private ExceptionLog CreateExceptionLog(Exception exception, int? userId, string severity, HttpContext? httpContext = null)
        {
            httpContext ??= _httpContextAccessor.HttpContext;

            return new ExceptionLog
            {
                UserId = userId,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                ExceptionType = exception.GetType().Name,
                Source = exception.Source,
                InnerException = exception.InnerException?.Message,
                StatusCode = httpContext?.Response.StatusCode,
                RequestPath = httpContext?.Request.Path.Value,
                HttpMethod = httpContext?.Request.Method,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                Severity = severity,
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task SaveExceptionLogAsync(ExceptionLog exceptionLog)
        {
            try
            {
                await _unitOfWork.ExceptionLogs.AddAsync(exceptionLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log error to console or file but don't throw - exception logging should not break the application
                Console.WriteLine($"Failed to save exception log: {exceptionLog.Message}");
            }
        }

        private string? GetClientIpAddress(HttpContext? context)
        {
            if (context == null) return null;

            // Try to get IP from X-Forwarded-For header first (for proxied requests)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Fall back to remote IP address
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}

