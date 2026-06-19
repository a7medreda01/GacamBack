using AppBL.Helper;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    // ─── Request DTOs ─────────────────────────────────────────────────────────
    public class SettingUpdateRequest
    {
        public int Id { get; set; }
        public string SiteTitleEn { get; set; } = string.Empty;
        public string SiteTitleAr { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string SocialLinksJson { get; set; } = "{}";
        public string ContactInfo { get; set; } = "{}";
    }

    public class CertificateDesignUpdateRequest
    {
        public int Id { get; set; }
        public string PrimaryColor { get; set; } = "#003F4A";
        public string SecondaryColor { get; set; } = "#C9A96B";
        public string BorderColor { get; set; } = "#003F4A";
        public float BorderWidth { get; set; } = 10f;
        public string TitleEn { get; set; } = "CERTIFICATE OF PARTICIPATION";
        public string TitleAr { get; set; } = "شهادة مشاركة تقديرية";
        public string HeaderTextEn { get; set; } = "GULF & ARAB GENERAL COMMISSION FOR AUDIOVISUAL MEDIA";
        public string HeaderTextAr { get; set; } = "الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا";
        public string? SignatoryName { get; set; }
        public string? SignatoryTitleEn { get; set; }
        public string? SignatoryTitleAr { get; set; }
        public string? SignatureImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public bool ShowLogo { get; set; } = true;
        public float LogoHeight { get; set; } = 60f;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;
        private readonly ICertificateDesignService _certificateDesignService;
        private readonly IFileHelper _fileHelper;

        public SettingsController(
            ISettingService settingService,
            ICertificateDesignService certificateDesignService,
            IFileHelper fileHelper)
        {
            _settingService = settingService;
            _certificateDesignService = certificateDesignService;
            _fileHelper = fileHelper;
        }

        // ─── General Settings ─────────────────────────────────────────────────

        /// <summary>Get general website settings (Title, Logo, Social links, Contact info).</summary>
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _settingService.GetAsync();
            return Ok(settings);
        }

        /// <summary>Update general website settings. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new AppDAL.Entities.Setting
            {
                Id              = request.Id,
                SiteTitleEn     = request.SiteTitleEn,
                SiteTitleAr     = request.SiteTitleAr,
                LogoUrl         = request.LogoUrl,
                SocialLinksJson = request.SocialLinksJson,
                ContactInfo     = request.ContactInfo
            };

            var settings = await _settingService.UpdateAsync(entity);
            return Ok(settings);
        }

        /// <summary>
        /// Upload the site logo image (PNG/JPG/SVG).
        /// The returned URL is automatically saved as LogoUrl in Settings.
        /// Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("upload-logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided." });

            var relativePath = await _fileHelper.UploadFileAsync(file, "logos");
            var absoluteUrl  = _fileHelper.ResolveUrl(relativePath);

            // Auto-save to settings
            var existing = await _settingService.GetAsync();
            existing.LogoUrl = relativePath;
            await _settingService.UpdateAsync(existing);

            return Ok(new { RelativePath = relativePath, AbsoluteUrl = absoluteUrl });
        }

        // ─── Certificate Design ────────────────────────────────────────────────

        /// <summary>Get certificate design configurations. Admin or Employee.</summary>
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("certificate")]
        public async Task<IActionResult> GetCertificateDesign()
        {
            var design = await _certificateDesignService.GetAsync();
            return Ok(design);
        }

        /// <summary>Update certificate design configurations. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("certificate")]
        public async Task<IActionResult> UpdateCertificateDesign([FromBody] CertificateDesignUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new AppDAL.Entities.CertificateDesign
            {
                Id                = request.Id,
                PrimaryColor      = request.PrimaryColor,
                SecondaryColor    = request.SecondaryColor,
                BorderColor       = request.BorderColor,
                BorderWidth       = request.BorderWidth,
                TitleEn           = request.TitleEn,
                TitleAr           = request.TitleAr,
                HeaderTextEn      = request.HeaderTextEn,
                HeaderTextAr      = request.HeaderTextAr,
                SignatoryName     = request.SignatoryName,
                SignatoryTitleEn  = request.SignatoryTitleEn,
                SignatoryTitleAr  = request.SignatoryTitleAr,
                SignatureImageUrl  = request.SignatureImageUrl,
                BackgroundImageUrl = request.BackgroundImageUrl,
                ShowLogo          = request.ShowLogo,
                LogoHeight        = request.LogoHeight
            };

            var design = await _certificateDesignService.UpdateAsync(entity);
            return Ok(design);
        }

        /// <summary>
        /// Upload the signatory signature image for certificates.
        /// The returned URL is automatically saved in CertificateDesign.SignatureImageUrl.
        /// Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("certificate/upload-signature")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSignature(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided." });

            var relativePath = await _fileHelper.UploadFileAsync(file, "signatures");
            var absoluteUrl  = _fileHelper.ResolveUrl(relativePath);

            // Auto-save to certificate design
            var existing = await _certificateDesignService.GetAsync();
            existing.SignatureImageUrl = relativePath;
            await _certificateDesignService.UpdateAsync(existing);

            return Ok(new { RelativePath = relativePath, AbsoluteUrl = absoluteUrl });
        }

        /// <summary>
        /// Upload a background image for certificates.
        /// When set, this image fills the certificate PDF background.
        /// When null / not uploaded, background defaults to plain white.
        /// Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("certificate/upload-background")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCertificateBackground(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided." });

            var relativePath = await _fileHelper.UploadFileAsync(file, "certificate-backgrounds");
            var absoluteUrl  = _fileHelper.ResolveUrl(relativePath);

            // Auto-save to certificate design
            var existing = await _certificateDesignService.GetAsync();
            existing.BackgroundImageUrl = relativePath;
            await _certificateDesignService.UpdateAsync(existing);

            return Ok(new { RelativePath = relativePath, AbsoluteUrl = absoluteUrl });
        }

        /// <summary>
        /// Remove the certificate background image (resets to plain white background).
        /// Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("certificate/background")]
        public async Task<IActionResult> RemoveCertificateBackground()
        {
            var existing = await _certificateDesignService.GetAsync();
            existing.BackgroundImageUrl = null;
            await _certificateDesignService.UpdateAsync(existing);
            return Ok(new { Message = "Background image removed. Certificates will use plain white background." });
        }
    }
}
