namespace AppBL.DTOs
{
    /// <summary>
    /// Unified verification response for certificates and media cards.
    /// </summary>
    public class UnifiedVerificationResponseDto
    {
        public bool IsValid { get; set; }
        public string? Type { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
    }

    public class CertificateVerificationDataDto
    {
        public int Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string FullNameOnCertificate { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? RelatedItemTitle { get; set; }
        public DateTime IssuedAt { get; set; }
        public string QrCodeData { get; set; } = string.Empty;
    }

    public class MediaCardVerificationDataDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public string CategoryNameAr { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired { get; set; }
        public string QrCodeData { get; set; } = string.Empty;
    }
}
