using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum PartnerCategory
    {
        Strategic,
        Supporting,
        Community,
        Media,
        EducationalAndCultural
    }

    public class Partner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? WebsiteUrl { get; set; }

        [Required]
        public PartnerCategory Category { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
