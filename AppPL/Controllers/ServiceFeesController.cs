using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceFeesController : ControllerBase
    {
        private readonly IServiceFeeService _serviceFeeService;

        public ServiceFeesController(IServiceFeeService serviceFeeService)
        {
            _serviceFeeService = serviceFeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fees = await _serviceFeeService.GetAllFeesAsync();
            return Ok(fees);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(OrderType type, [FromBody] ServiceFeeUpdateRequest request)
        {
            var updatedFee = await _serviceFeeService.UpdateFeeAsync(type, request);
            return Ok(updatedFee);
        }
    }
}
