using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class NewsDto
    {
        public int Id { get; set; }
        public NewsType Type { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string ContentEn { get; set; } = string.Empty;
        public string ContentAr { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class NewsCreateRequest
    {
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

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class NewsUpdateRequest
    {
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

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
