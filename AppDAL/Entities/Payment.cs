using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppDAL.Entities
{
    public enum PaymentStatus
    {
        PendingVerification,
        Paid,
        Rejected,
        Refunded
    }

    public enum PaymentType
    {
        Accreditation,
        Course,
        Certificate,
        PrintOrder
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(150)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty; // Transaction Ref

        [Required]
        [MaxLength(500)]
        public string ReceiptUrl { get; set; } = string.Empty; // path to uploaded image

        [Required]
        public PaymentType Type { get; set; }

        [Required]
        public int RelatedRecordId { get; set; } // AccreditationId, CourseId, or CertificateRequest (CourseId/VolunteerId)

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.PendingVerification;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }

        public int? VerifiedByUserId { get; set; }
        public User? VerifiedByUser { get; set; }

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        public Order? Order { get; set; }
    }
}
