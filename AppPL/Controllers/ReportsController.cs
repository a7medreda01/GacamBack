using AppBL.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Employee")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("payments")]
        public async Task<IActionResult> ExportPayments()
        {
            var bytes = await _reportService.GeneratePaymentsReportAsync();
            string fileName = $"GACAM_Payments_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("auditlogs")]
        public async Task<IActionResult> ExportAuditLogs()
        {
            var bytes = await _reportService.GenerateAuditLogsReportAsync();
            string fileName = $"GACAM_AuditLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("users")]
        public async Task<IActionResult> ExportUsers()
        {
            var bytes = await _reportService.GenerateUsersReportAsync();
            string fileName = $"GACAM_Users_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
