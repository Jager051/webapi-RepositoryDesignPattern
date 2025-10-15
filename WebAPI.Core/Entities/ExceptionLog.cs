using System.ComponentModel.DataAnnotations;

namespace WebAPI.Core.Entities
{
    /// <summary>
    /// Exception log entity for tracking application errors
    /// </summary>
    public class ExceptionLog : BaseEntity
    {
        /// <summary>
        /// User ID who encountered the exception (nullable for anonymous users)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Exception message
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception stack trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception type (e.g., NullReferenceException, ArgumentException)
        /// </summary>
        [MaxLength(200)]
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Source of the exception (controller, service, etc.)
        /// </summary>
        [MaxLength(500)]
        public string? Source { get; set; }

        /// <summary>
        /// Inner exception message if exists
        /// </summary>
        [MaxLength(2000)]
        public string? InnerException { get; set; }

        /// <summary>
        /// HTTP status code if applicable
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Request path where exception occurred
        /// </summary>
        [MaxLength(500)]
        public string? RequestPath { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, etc.)
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

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
        /// Severity level (Critical, Error, Warning)
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Error";

        /// <summary>
        /// Navigation property to User
        /// </summary>
        public virtual User? User { get; set; }
    }
}

