using Microsoft.AspNetCore.Http;
using System.Text.Json;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Services
{
    /// <summary>
    /// Service for managing audit logs
    /// </summary>
    public class AuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Log a create action
        /// </summary>
        public async Task LogCreateAsync<T>(string entityName, int entityId, T newEntity, int? userId = null)
        {
            var auditLog = CreateBaseAuditLog(userId, "Create", entityName, entityId);
            auditLog.Description = $"{entityName} created";
            auditLog.NewValues = JsonSerializer.Serialize(newEntity);
            auditLog.LogLevel = "Information";

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log an update action
        /// </summary>
        public async Task LogUpdateAsync<T>(string entityName, int entityId, T oldEntity, T newEntity, int? userId = null)
        {
            var auditLog = CreateBaseAuditLog(userId, "Update", entityName, entityId);
            auditLog.Description = $"{entityName} updated";
            auditLog.OldValues = JsonSerializer.Serialize(oldEntity);
            auditLog.NewValues = JsonSerializer.Serialize(newEntity);
            auditLog.LogLevel = "Information";

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log a delete action
        /// </summary>
        public async Task LogDeleteAsync<T>(string entityName, int entityId, T deletedEntity, int? userId = null)
        {
            var auditLog = CreateBaseAuditLog(userId, "Delete", entityName, entityId);
            auditLog.Description = $"{entityName} deleted";
            auditLog.OldValues = JsonSerializer.Serialize(deletedEntity);
            auditLog.LogLevel = "Warning";

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log a custom action
        /// </summary>
        public async Task LogActionAsync(string action, string? entityName = null, int? entityId = null, 
            string? description = null, object? oldValues = null, object? newValues = null, 
            string logLevel = "Information", int? userId = null)
        {
            var auditLog = CreateBaseAuditLog(userId, action, entityName, entityId);
            auditLog.Description = description;
            auditLog.OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
            auditLog.NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null;
            auditLog.LogLevel = logLevel;

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log user login
        /// </summary>
        public async Task LogLoginAsync(int userId, bool success)
        {
            var auditLog = CreateBaseAuditLog(userId, success ? "Login" : "LoginFailed");
            auditLog.Description = success ? "User logged in successfully" : "Failed login attempt";
            auditLog.LogLevel = success ? "Information" : "Warning";

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log user logout
        /// </summary>
        public async Task LogLogoutAsync(int userId)
        {
            var auditLog = CreateBaseAuditLog(userId, "Logout");
            auditLog.Description = "User logged out";
            auditLog.LogLevel = "Information";

            await SaveAuditLogAsync(auditLog);
        }

        /// <summary>
        /// Log user registration
        /// </summary>
        public async Task LogRegistrationAsync(int userId, string username, string email)
        {
            var auditLog = CreateBaseAuditLog(userId, "Register");
            auditLog.Description = $"New user registered: {username}";
            auditLog.EntityName = "User";
            auditLog.EntityId = userId;
            auditLog.NewValues = JsonSerializer.Serialize(new { Username = username, Email = email });
            auditLog.LogLevel = "Information";

            await SaveAuditLogAsync(auditLog);
        }

        private AuditLog CreateBaseAuditLog(int? userId, string action, string? entityName = null, int? entityId = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var startTime = DateTime.UtcNow;

            return new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                RequestPath = httpContext?.Request.Path.Value,
                HttpMethod = httpContext?.Request.Method,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                StatusCode = httpContext?.Response.StatusCode,
                CreatedAt = startTime
            };
        }

        private async Task SaveAuditLogAsync(AuditLog auditLog)
        {
            try
            {
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log error but don't throw - audit logging should not break the application
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

