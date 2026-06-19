using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    /// <summary>
    /// User profile management including image upload.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IAuthService _authService;

        public ProfileController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Get the authenticated user's profile.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.GetProfileAsync(userId.Value);
            return Ok(profile);
        }

        /// <summary>Update the authenticated user's profile.</summary>
        [HttpPut]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.UpdateProfileAsync(userId.Value, request);
            return Ok(profile);
        }

        /// <summary>Upload or replace profile image. Allowed formats: JPG, JPEG, PNG, WEBP. Max size: 5 MB.</summary>
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.UploadProfileImageAsync(userId.Value, file);
            return Ok(profile);
        }

        private int? GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }
    }
}
