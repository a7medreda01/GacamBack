using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string TitleEn { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public decimal FeeAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseCreateRequest
    {
        [Required]
        [MaxLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required]
        public string DescriptionAr { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal FeeAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CourseUpdateRequest
    {
        [Required]
        [MaxLength(200)]
        public string TitleEn { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TitleAr { get; set; } = string.Empty;

        [Required]
        public string DescriptionEn { get; set; } = string.Empty;

        [Required]
        public string DescriptionAr { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal FeeAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }

    public class CourseEnrollmentDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitleEn { get; set; } = string.Empty;
        public string CourseTitleAr { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public EnrollmentStatus Status { get; set; }
        public int? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EnrollmentRequest
    {
        [Required]
        public int CourseId { get; set; }
    }

    public class EnrollmentStatusRequest
    {
        [Required]
        public EnrollmentStatus Status { get; set; }
    }
}
