using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificatesController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RequestCertificate([FromBody] CertificateRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var certificate = await _certificateService.RequestCertificateAsync(userId, request);
            return Ok(certificate);
        }

        [Authorize]
        [HttpGet("my-certificates")]
        public async Task<IActionResult> GetMyCertificates([FromQuery] PagedRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var certificates = await _certificateService.GetUserCertificatesAsync(userId, request);
            return Ok(certificates);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request)
        {
            var certificates = await _certificateService.GetAllCertificatesAsync(request);
            return Ok(certificates);
        }

        [HttpGet("verify/{number}")]
        public async Task<IActionResult> Verify(string number)
        {
            var verification = await _certificateService.VerifyCertificateAsync(number);
            return Ok(verification);
        }

        /// <summary>
        /// Verify certificate validity by uploading a certificate file (PDF or Image).
        /// The server parses the file, extracts the embedded QR code, decodes it, and validates the certificate.
        /// </summary>
        [HttpPost("verify-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> VerifyFile(IFormFile file)
        {
            var verification = await _certificateService.VerifyCertificateFileAsync(file);
            return Ok(verification);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var cert = await _certificateService.GetCertificateByIdAsync(id);
            if (cert == null)
                return NotFound(new { Message = "Certificate not found." });

            // Generate PDF in memory — no file read/write
            var pdfBytes = await _certificateService.GenerateCertificatePdfBytesAsync(id);
            return File(pdfBytes, "application/pdf", $"certificate_{cert.CertificateNumber}.pdf");
        }
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var certificate = await _certificateService.GetCertificateByIdAsync(id);

            if (certificate == null)
                return NotFound(new
                {
                    Message = "Certificate not found."
                });

            return Ok(certificate);
        }
    }
}

