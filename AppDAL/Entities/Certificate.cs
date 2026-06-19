using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum CertificateType
    {
        Training,
        Volunteer,
        Participation,
        Custom
    }

    public class Certificate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public CertificateType Type { get; set; }

        public int? RelatedRecordId { get; set; } // CourseId for Training, VolunteerId for Volunteer, etc.

        [Required]
        [MaxLength(200)]
        public string FullNameOnCertificate { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CertificateNumber { get; set; } = string.Empty; // Unique certificate number

        [Required]
        [MaxLength(500)]
        public string QrCodeData { get; set; } = string.Empty; // Verification URL

        [Required]
        [MaxLength(500)]
        public string PdfUrl { get; set; } = string.Empty; // Path to generated PDF file

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    }
}
