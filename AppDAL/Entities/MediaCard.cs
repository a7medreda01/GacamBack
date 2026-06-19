using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum CardStatus
    {
        Active,
        Expired,
        Suspended,
        Revoked
    }

    public class MediaCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccreditationId { get; set; }
        public MediaAccreditation Accreditation { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string CardNumber { get; set; } = string.Empty; // Unique card number

        [Required]
        [MaxLength(500)]
        public string QrCodeData { get; set; } = string.Empty; // Verification URL or data

        [Required]
        public CardStatus Status { get; set; } = CardStatus.Active;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }
    }
}
