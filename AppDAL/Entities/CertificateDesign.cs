using System;
using System.ComponentModel.DataAnnotations;

namespace AppDAL.Entities
{
    /// <summary>
    /// Represents the customizable styling and content configuration for generated certificates.
    /// Used by admin panel to dynamically control QuestPDF layout.
    /// </summary>
    public class CertificateDesign
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public string PrimaryColor { get; set; } = "#003F4A"; // Teal

        [MaxLength(20)]
        public string SecondaryColor { get; set; } = "#C9A96B"; // Gold

        [MaxLength(20)]
        public string BorderColor { get; set; } = "#003F4A";

        public float BorderWidth { get; set; } = 10f;

        [MaxLength(200)]
        public string TitleEn { get; set; } = "CERTIFICATE OF PARTICIPATION";

        [MaxLength(200)]
        public string TitleAr { get; set; } = "شهادة مشاركة تقديرية";

        [MaxLength(500)]
        public string HeaderTextEn { get; set; } = "GULF & ARAB GENERAL COMMISSION FOR AUDIOVISUAL MEDIA";

        [MaxLength(500)]
        public string HeaderTextAr { get; set; } = "الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا";

        [MaxLength(200)]
        public string? SignatoryName { get; set; } = "Executive Director";

        [MaxLength(200)]
        public string? SignatoryTitleEn { get; set; } = "GACAM Administration";

        [MaxLength(200)]
        public string? SignatoryTitleAr { get; set; } = "إدارة الهيئة العامة للإعلام";

        [MaxLength(500)]
        public string? SignatureImageUrl { get; set; }

        public bool ShowLogo { get; set; } = true;

        public float LogoHeight { get; set; } = 60f;

        /// <summary>
        /// Optional background image for the certificate PDF.
        /// When null, the background will be plain white.
        /// </summary>
        [MaxLength(500)]
        public string? BackgroundImageUrl { get; set; }
    }
}
