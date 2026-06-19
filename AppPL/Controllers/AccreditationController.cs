using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccreditationController : ControllerBase
    {
        private readonly IMediaAccreditationService _accreditationService;

        public AccreditationController(IMediaAccreditationService accreditationService)
        {
            _accreditationService = accreditationService;
        }

        [Authorize]
        [HttpPost("apply")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Apply([FromForm] AccreditationApplyRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var accreditation = await _accreditationService.ApplyAccreditationAsync(userId, request);
            return Ok(accreditation);
        }

        [Authorize]
        [HttpGet("my-application")]
        public async Task<IActionResult> GetMyApplication()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var application = await _accreditationService.GetAccreditationByUserIdAsync(userId);
            if (application == null)
                return NotFound(new { Message = "No accreditation application found for your account." });

            return Ok(application);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] ApplicationStatus? status)
        {
            var accreditations = await _accreditationService.GetAllAccreditationsAsync(request, status);
            return Ok(accreditations);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var accreditation = await _accreditationService.GetAccreditationByIdAsync(id);
            if (accreditation == null)
                return NotFound(new { Message = "Accreditation application not found." });

            return Ok(accreditation);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(int id, [FromBody] AccreditationReviewRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var updatedAccreditation = await _accreditationService.ReviewAccreditationAsync(id, userId, request);
            return Ok(updatedAccreditation);
        }

        [HttpGet("verify/card/{number}")]
        public async Task<IActionResult> VerifyCard(string number)
        {
            var result = await _accreditationService.VerifyCardAsync(number);
            return Ok(result);
        }
    }
}
