using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IFileHelper _fileHelper;

        public PaymentsController(IPaymentService paymentService, IFileHelper fileHelper)
        {
            _paymentService = paymentService;
            _fileHelper = fileHelper;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitPayment([FromBody] PaymentSubmitRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var payment = await _paymentService.SubmitPaymentAsync(userId, request);
            return Ok(payment);
        }

        [Authorize]
        [HttpPost("upload-receipt")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadReceipt(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var relativePath = await _fileHelper.UploadFileAsync(file, "receipts");
            var absoluteUrl = _fileHelper.ResolveUrl(relativePath);
            
            return Ok(new { RelativePath = relativePath, AbsoluteUrl = absoluteUrl });
        }

        [Authorize]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments([FromQuery] PagedRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var payments = await _paymentService.GetUserPaymentsAsync(userId, request);
            return Ok(payments);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] PaymentStatus? status)
        {
            var payments = await _paymentService.GetAllPaymentsAsync(request, status);
            return Ok(payments);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);

            if (payment == null)
                return NotFound(new { Message = "Payment record not found." });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var isAdminOrEmployee =
                User.IsInRole("Admin") ||
                User.IsInRole("Employee");

            if (!isAdminOrEmployee && payment.UserId != userId)
            {
                return Forbid();
            }

            return Ok(payment);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(int id, [FromBody] PaymentReviewRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var updatedPayment = await _paymentService.ReviewPaymentAsync(id, userId, request);
            return Ok(updatedPayment);
        }
    }
}
