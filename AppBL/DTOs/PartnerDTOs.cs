using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class PartnerDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public PartnerCategory Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PartnerCreateRequest
    {
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
    }

    public class PartnerUpdateRequest
    {
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

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
