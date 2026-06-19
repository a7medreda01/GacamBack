using AppDAL.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class AccreditationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryNameEn { get; set; } = string.Empty;
        public string CategoryNameAr { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? CheckedByUserName { get; set; }
        public MediaCardDto? MediaCard { get; set; }
    }

    public class MediaCardDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string QrCodeData { get; set; } = string.Empty;
        public CardStatus Status { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class AccreditationApplyRequest
    {
        [Required]
        public int AccreditationCategoryId { get; set; }

        [Required]
        public IFormFile Document { get; set; } = null!;
    }

    public class AccreditationReviewRequest
    {
        [Required]
        public ApplicationStatus Status { get; set; }
    }

    public class CardVerifyDto
    {
        public bool IsValid { get; set; }
        public string? CardNumber { get; set; }
        public string? FullName { get; set; }
        public string? CategoryNameEn { get; set; }
        public string? CategoryNameAr { get; set; }
        public string? Status { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
