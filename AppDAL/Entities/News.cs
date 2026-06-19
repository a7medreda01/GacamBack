using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum NewsType
    {
        News,
        PressRelease,
        Announcement,
        Statement,
        EventAndForum,
        Initiative
    }

    public class News
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public NewsType Type { get; set; }

        [Required]
        [MaxLength(250)]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string ContentEn { get; set; } = string.Empty;

        [Required]
        public string ContentAr { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public int ViewCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
