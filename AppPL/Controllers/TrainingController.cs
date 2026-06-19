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
    public class TrainingController : ControllerBase
    {
        private readonly ITrainingService _trainingService;

        public TrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses([FromQuery] PagedRequestDto request, [FromQuery] bool? activeOnly)
        {
            var courses = await _trainingService.GetAllCoursesAsync(request, activeOnly);
            return Ok(courses);
        }

        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _trainingService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound(new { Message = "Course not found." });

            return Ok(course);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
        {
            var course = await _trainingService.CreateCourseAsync(request);
            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateRequest request)
        {
            var updatedCourse = await _trainingService.UpdateCourseAsync(id, request);
            return Ok(updatedCourse);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _trainingService.DeleteCourseAsync(id);
            if (!result)
                return NotFound(new { Message = "Course not found." });

            return Ok(new { Message = "Course deleted successfully." });
        }

        [Authorize]
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var enrollment = await _trainingService.EnrollInCourseAsync(userId, request);
            return Ok(enrollment);
        }

        [Authorize]
        [HttpGet("my-enrollments")]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] PagedRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var enrollments = await _trainingService.GetUserEnrollmentsAsync(userId, request);
            return Ok(enrollments);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("enrollments")]
        public async Task<IActionResult> GetAllEnrollments([FromQuery] PagedRequestDto request, [FromQuery] EnrollmentStatus? status)
        {
            var enrollments = await _trainingService.GetAllEnrollmentsAsync(request, status);
            return Ok(enrollments);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("enrollments/{id}/status")]
        public async Task<IActionResult> UpdateEnrollmentStatus(int id, [FromBody] EnrollmentStatusRequest request)
        {
            var updatedEnrollment = await _trainingService.UpdateEnrollmentStatusAsync(id, request);
            return Ok(updatedEnrollment);
        }
    }
}
