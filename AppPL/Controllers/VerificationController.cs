using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    /// <summary>
    /// Unified verification endpoint for certificates and media cards.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        /// <summary>
        /// Verify a certificate or media card by certificate number, card number, or QR code data.
        /// </summary>
        /// <param name="code">Certificate number, card number, or QR code value.</param>
        /// <response code="200">Returns verification result with type Certificate, MediaCard, or not found message.</response>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(UnifiedVerificationResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Verify(string code)
        {
            var result = await _verificationService.VerifyAsync(code);
            return Ok(result);
        }
        [HttpPost("verify-files")]
        public async Task<IActionResult> VerifyFile([FromForm] IFormFile file)
        {
            var result = await _verificationService.VerifyFileAsync(file);
            return Ok(result);
        }
    }
}
