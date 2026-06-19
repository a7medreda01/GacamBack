using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public class Page
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty; // unique key

        [Required]
        [MaxLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string ContentEn { get; set; } = string.Empty;

        [Required]
        public string ContentAr { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
