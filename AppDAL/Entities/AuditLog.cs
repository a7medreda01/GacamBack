using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [MaxLength(150)]
        public string? UserEmail { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // e.g. "Create", "Update", "Delete", "Login"

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        public string? RecordId { get; set; }

        public string? OldValues { get; set; } // JSON format

        public string? NewValues { get; set; } // JSON format

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? IpAddress { get; set; }
    }
}
