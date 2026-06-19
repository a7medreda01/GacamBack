using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum EnrollmentStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class CourseEnrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
