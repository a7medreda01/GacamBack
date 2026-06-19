using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var profile = await _authService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { Message = "Password changed successfully." });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PagedRequestDto request)
        {
            var users = await _authService.GetAllUsersWithRolesAsync(request);
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{id}/roles")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
        {
            await _authService.AssignRoleAsync(id, request.RoleName);
            return Ok(new { Message = $"Role '{request.RoleName}' assigned to user successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}/roles")]
        public async Task<IActionResult> RemoveRole(int id, [FromQuery] string roleName)
        {
            await _authService.RemoveRoleAsync(id, roleName);
            return Ok(new { Message = $"Role '{roleName}' removed from user successfully." });
        }
    }
}
