using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppPL.Controllers
{
    /// <summary>
    /// Manages print orders for certificates and accreditation cards.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>Create a new print order. Fees are calculated from ServiceFees automatically.</summary>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var order = await _orderService.CreateOrderAsync(userId.Value, request);
            return Ok(order);
        }

        /// <summary>Get current user's orders (paginated).</summary>
        [Authorize]
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(PagedResponse<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders([FromQuery] PagedRequestDto request, [FromQuery] OrderStatus? status)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId.Value, request, status);
            return Ok(orders);
        }

        /// <summary>Get all orders (Admin only, paginated).</summary>
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] OrderStatus? status)
        {
            var orders = await _orderService.GetAllOrdersAsync(request, status);
            return Ok(orders);
        }

        /// <summary>Get order by ID. Users can only access their own orders.</summary>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(id, userId, IsAdminOrEmployee());
            if (order == null)
                return NotFound(new { Message = "Order not found." });

            return Ok(order);
        }

        /// <summary>Update an order.</summary>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var order = await _orderService.UpdateOrderAsync(id, userId.Value, request, IsAdminOrEmployee());
            return Ok(order);
        }

        /// <summary>Delete an order.</summary>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var deleted = await _orderService.DeleteOrderAsync(id, userId.Value, IsAdminOrEmployee());
            if (!deleted)
                return NotFound(new { Message = "Order not found." });

            return Ok(new { Message = "Order deleted successfully." });
        }

        /// <summary>Update order status (Admin only).</summary>
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var order = await _orderService.UpdateOrderStatusAsync(id, userId.Value, request, true);
            return Ok(order);
        }

        /// <summary>Get order status timeline/history.</summary>
        [Authorize]
        [HttpGet("{id}/timeline")]
        [ProducesResponseType(typeof(IEnumerable<OrderStatusHistoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTimeline(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var timeline = await _orderService.GetOrderTimelineAsync(id, userId, IsAdminOrEmployee());
            return Ok(timeline);
        }

        /// <summary>Link a payment to an order.</summary>
        [Authorize]
        [HttpPost("{id}/link-payment/{paymentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LinkPayment(int id, int paymentId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _orderService.LinkPaymentToOrderAsync(id, paymentId, userId.Value);
            return Ok(new { Message = "Payment linked to order successfully." });
        }

        private int? GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }

        private bool IsAdminOrEmployee() =>
            User.IsInRole("Admin") || User.IsInRole("Employee");
    }
}
