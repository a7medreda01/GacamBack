using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    public enum VolunteeringArea
    {
        MediaAndJournalism,
        PhotographyAndProduction,
        PublicRelations,
        EventManagement,
        TranslationAndEditing,
        DesignAndCreativeServices,
        DigitalMedia,
        TrainingPrograms,
        AdministrativeSupport
    }

    public class Volunteer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

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
        public string CVUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Skills { get; set; }

        [Required]
        public VolunteeringArea Area { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
