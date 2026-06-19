using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    /// <summary>
    /// Database-backed accreditation category (replaces legacy enum).
    /// </summary>
    public class AccreditationCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DescriptionEn { get; set; }

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MediaAccreditation> MediaAccreditations { get; set; } = new List<MediaAccreditation>();
    }
}
