using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnersController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] PartnerCategory? category)
        {
            var partners = await _partnerService.GetAllPartnersAsync(request, category);
            return Ok(partners);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(id);
            if (partner == null)
                return NotFound(new { Message = "Partner not found." });

            return Ok(partner);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartnerCreateRequest request)
        {
            var partner = await _partnerService.CreatePartnerAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = partner.Id }, partner);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartnerUpdateRequest request)
        {
            var updatedPartner = await _partnerService.UpdatePartnerAsync(id, request);
            return Ok(updatedPartner);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _partnerService.DeletePartnerAsync(id);
            if (!result)
                return NotFound(new { Message = "Partner not found." });

            return Ok(new { Message = "Partner deleted successfully." });
        }
    }
}
