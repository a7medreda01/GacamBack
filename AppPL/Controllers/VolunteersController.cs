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
    public class VolunteersController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteersController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] VolunteerRegisterRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var volunteer = await _volunteerService.RegisterVolunteerAsync(userId, request);
            return Ok(volunteer);
        }

        [Authorize]
        [HttpGet("my-application")]
        public async Task<IActionResult> GetMyApplication()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var application = await _volunteerService.GetVolunteerByUserIdAsync(userId);
            if (application == null)
                return NotFound(new { Message = "You have not submitted a volunteer application." });

            return Ok(application);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ApplicationStatus? status)
        {
            var applications = await _volunteerService.GetAllVolunteersAsync(status);
            return Ok(applications);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(id);
            if (volunteer == null)
                return NotFound(new { Message = "Volunteer application not found." });

            return Ok(volunteer);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] VolunteerStatusUpdateRequest request)
        {
            var updatedVolunteer = await _volunteerService.UpdateVolunteerStatusAsync(id, request);
            return Ok(updatedVolunteer);
        }
    }
}
