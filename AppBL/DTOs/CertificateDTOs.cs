using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public CertificateType Type { get; set; }
        public int? RelatedRecordId { get; set; }
        public string FullNameOnCertificate { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public string QrCodeData { get; set; } = string.Empty;
        public string PdfUrl { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsExpired { get; set; }
    }

    public class CertificateRequestDto
    {
        [Required]
        public CertificateType Type { get; set; }

        public int? RelatedRecordId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullNameOnCertificate { get; set; } = string.Empty;

        public bool RequestPrinted { get; set; }
    }

    public class CertificateVerifyDto
    {
        public bool IsValid { get; set; }
        public string? CertificateNumber { get; set; }
        public string? FullNameOnCertificate { get; set; }
        public string? Type { get; set; }
        public string? RelatedItemTitle { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool IsExpired { get; set; }
    }
}
