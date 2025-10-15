using System.ComponentModel.DataAnnotations;

namespace WebAPI.Core.Entities
{
    /// <summary>
    /// Audit log entity for tracking user actions and system events
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// User ID who performed the action (nullable for system actions)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Action performed (Create, Update, Delete, Login, Logout, etc.)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Entity/Table name that was affected
        /// </summary>
        [MaxLength(100)]
        public string? EntityName { get; set; }

        /// <summary>
        /// ID of the affected entity
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Description of the action
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Old values (JSON format for updates/deletes)
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// New values (JSON format for creates/updates)
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (browser info)
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request path
        /// </summary>
        [MaxLength(500)]
        public string? RequestPath { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE)
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Duration of the request in milliseconds
        /// </summary>
        public long? Duration { get; set; }

        /// <summary>
        /// Log level (Information, Warning, Error)
        /// </summary>
        [MaxLength(20)]
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Navigation property to User
        /// </summary>
        public virtual User? User { get; set; }
    }
}

