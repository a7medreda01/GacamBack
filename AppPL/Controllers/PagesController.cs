using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request)
        {
            var pages = await _pageService.GetAllPagesAsync(request);
            return Ok(pages);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var page = await _pageService.GetPageBySlugAsync(slug);
            if (page == null)
                return NotFound(new { Message = $"Page with slug '{slug}' not found." });

            return Ok(page);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{slug}")]
        public async Task<IActionResult> Update(string slug, [FromBody] PageUpdateRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var updatedPage = await _pageService.UpdatePageAsync(slug, request, userId);
            return Ok(updatedPage);
        }
    }
}
