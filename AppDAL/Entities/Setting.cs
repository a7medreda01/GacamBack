using System;

namespace AppDAL.Entities
{
    public class Setting
    {
        public int Id { get; set; }
        public string SiteTitleEn { get; set; } = string.Empty;
        public string SiteTitleAr { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty; // relative path in wwwroot
        public string SocialLinksJson { get; set; } = string.Empty; // e.g., {"facebook":"...","twitter":"..."}
        public string ContactInfo { get; set; } = string.Empty; // e.g., phone, email
    }
}
