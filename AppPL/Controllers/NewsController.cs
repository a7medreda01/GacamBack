using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] NewsType? type)
        {
            var newsList = await _newsService.GetAllNewsAsync(request, type);
            return Ok(newsList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
                return NotFound(new { Message = "News article not found." });

            return Ok(news);
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementView(int id)
        {
            var result = await _newsService.IncrementViewCountAsync(id);
            if (!result)
                return NotFound(new { Message = "News article not found." });

            return Ok(new { Message = "View count incremented." });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewsCreateRequest request)
        {
            var news = await _newsService.CreateNewsAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = news.Id }, news);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewsUpdateRequest request)
        {
            var updatedNews = await _newsService.UpdateNewsAsync(id, request);
            return Ok(updatedNews);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _newsService.DeleteNewsAsync(id);
            if (!result)
                return NotFound(new { Message = "News article not found." });

            return Ok(new { Message = "News article deleted successfully." });
        }
    }
}
