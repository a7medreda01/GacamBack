using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReceiptUrl { get; set; } = string.Empty;
        public PaymentType Type { get; set; }
        public int RelatedRecordId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByUserName { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class PaymentSubmitRequest
    {
        [Required]
        [Range(1, 100000)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(150)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ReceiptUrl { get; set; } = string.Empty; // Receipt file upload link

        [Required]
        public PaymentType Type { get; set; }

        [Required]
        public int RelatedRecordId { get; set; } // AccreditationId, CourseId, or CertificateRequest
    }

    public class PaymentReviewRequest
    {
        [Required]
        public PaymentStatus Status { get; set; } // Paid, Rejected, etc.

        [MaxLength(500)]
        public string? AdminNotes { get; set; }
    }
}
