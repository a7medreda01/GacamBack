using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto request);
        Task<OrderDto?> GetOrderByIdAsync(int id, int? userId, bool isAdmin);
        Task<PagedResponse<OrderDto>> GetUserOrdersAsync(int userId, PagedRequestDto request, OrderStatus? status = null);
        Task<PagedResponse<OrderDto>> GetAllOrdersAsync(PagedRequestDto request, OrderStatus? status = null);
        Task<OrderDto> UpdateOrderAsync(int id, int userId, UpdateOrderDto request, bool isAdmin);
        Task<bool> DeleteOrderAsync(int id, int userId, bool isAdmin);
        Task<OrderDto> UpdateOrderStatusAsync(int id, int changedByUserId, UpdateOrderStatusDto request, bool isAdmin);
        Task<IEnumerable<OrderStatusHistoryDto>> GetOrderTimelineAsync(int id, int? userId, bool isAdmin);
        Task LinkPaymentToOrderAsync(int orderId, int paymentId, int userId);
    }
}
