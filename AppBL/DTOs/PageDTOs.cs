using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class PageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string ContentEn { get; set; } = string.Empty;
        public string ContentAr { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedByUserName { get; set; }
    }

    public class PageUpdateRequest
    {
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

        public string? ImageUrl { get; set; }
    }
}
