using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected,
        Refunded
    }

    public class MediaAccreditation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int AccreditationCategoryId { get; set; }
        public AccreditationCategory AccreditationCategory { get; set; } = null!;

        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Required]
        [MaxLength(500)]
        public string DocumentUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CheckedAt { get; set; }

        public int? CheckedByUserId { get; set; }
        public User? CheckedByUser { get; set; }

        public MediaCard? MediaCard { get; set; }
    }
}
