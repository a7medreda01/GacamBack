using AppDAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace AppBL.DTOs
{
    public class VolunteerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CVUrl { get; set; } = string.Empty;
        public string? Skills { get; set; }
        public VolunteeringArea Area { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class VolunteerRegisterRequest
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CVUrl { get; set; } = string.Empty; // CV file upload link

        [MaxLength(500)]
        public string? Skills { get; set; }

        [Required]
        public VolunteeringArea Area { get; set; }
    }

    public class VolunteerStatusUpdateRequest
    {
        [Required]
        public ApplicationStatus Status { get; set; }
    }
}
