using System;

namespace AppBL.Dto
{
    /// <summary>
    /// Data Transfer Object for site settings.
    /// </summary>
    public class SettingDto
    {
        public int Id { get; set; }
        public string SiteTitleEn { get; set; } = string.Empty;
        public string SiteTitleAr { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? SocialLinksJson { get; set; }
        public string? ContactInfo { get; set; }
    }
}
