using AppBL.DTOs;
using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    /// <summary>
    /// CRUD operations for accreditation categories.
    /// </summary>
    [ApiController]
    [Route("api/accreditation-categories")]
    public class AccreditationCategoriesController : ControllerBase
    {
        private readonly IAccreditationCategoryService _categoryService;

        public AccreditationCategoriesController(IAccreditationCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get paginated accreditation categories.
        /// Example search: "Speaker" or "متحدث".
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<AccreditationCategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] bool? isActive)
        {
            var categories = await _categoryService.GetAllAsync(request, isActive);
            return Ok(categories);
        }

        /// <summary>Get accreditation category by ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AccreditationCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { Message = "Accreditation category not found." });

            return Ok(category);
        }

        /// <summary>Create a new accreditation category (Admin only).</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(AccreditationCategoryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateAccreditationCategoryDto request)
        {
            var category = await _categoryService.CreateAsync(request);
            return Ok(category);
        }

        /// <summary>Update an accreditation category (Admin only).</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AccreditationCategoryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccreditationCategoryDto request)
        {
            var category = await _categoryService.UpdateAsync(id, request);
            return Ok(category);
        }

        /// <summary>Delete an accreditation category (Admin only).</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { Message = "Accreditation category not found." });

            return Ok(new { Message = "Accreditation category deleted successfully." });
        }
    }
}
