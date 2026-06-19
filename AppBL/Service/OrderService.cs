using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(
            int userId,
            CreateOrderDto request)
        {
            await ValidatePrintOrderAsync(
                userId,
                request.OrderType,
                request.RelatedRecordId);

            var serviceFee = await _unitOfWork.ServiceFees
                .GetQueryable()
                .FirstOrDefaultAsync(x =>
                    x.OrderType == request.OrderType &&
                    x.IsActive);

            if (serviceFee == null)
                throw new InvalidOperationException(
                    $"No active pricing configured for order type '{request.OrderType}'.");

            var unitPrice = serviceFee.UnitPrice;
            var shippingFee = serviceFee.ShippingFee;

            var totalAmount =
                (unitPrice * request.Quantity)
                + shippingFee;

            var orderNumber = await GenerateOrderNumberAsync();

            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,

                OrderType = request.OrderType,
                RelatedRecordId = request.RelatedRecordId,

                Quantity = request.Quantity,

                UnitPrice = unitPrice,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,

                OrderStatus = OrderStatus.WaitingPayment,

                Notes = request.Notes,
                Phone=request.Phone,
                Address=request.Address,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            await AddStatusHistoryAsync(
                order.Id,
                null,
                OrderStatus.WaitingPayment,
                userId,
                "Order created.");

            return await GetOrderDtoByIdAsync(order.Id);
        }
        public async Task<OrderDto?> GetOrderByIdAsync(int id, int? userId, bool isAdmin)
        {
            var order = await GetOrderQuery()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this order.");

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<PagedResponse<OrderDto>> GetUserOrdersAsync(int userId, PagedRequestDto request, OrderStatus? status = null)
        {
            var query = ApplyOrderSearch(GetOrderQuery().Where(o => o.UserId == userId), request.Search);

            if (status.HasValue)
                query = query.Where(o => o.OrderStatus == status.Value);

            query = query.OrderByDescending(o => o.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return MapOrderPage(paged);
        }

        public async Task<PagedResponse<OrderDto>> GetAllOrdersAsync(PagedRequestDto request, OrderStatus? status = null)
        {
            var query = ApplyOrderSearch(GetOrderQuery(), request.Search);

            if (status.HasValue)
                query = query.Where(o => o.OrderStatus == status.Value);

            query = query.OrderByDescending(o => o.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return MapOrderPage(paged);
        }

        public async Task<OrderDto> UpdateOrderAsync(int id, int userId, UpdateOrderDto request, bool isAdmin)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this order.");

            if (!isAdmin && order.OrderStatus != OrderStatus.WaitingPayment && order.OrderStatus != OrderStatus.Pending)
                throw new InvalidOperationException("Order can only be updated before payment submission.");

            if (request.Quantity.HasValue && request.Quantity.Value != order.Quantity)
            {
                order.Quantity = request.Quantity.Value;
                order.TotalAmount = order.UnitPrice * order.Quantity;
            }

            if (request.Notes != null)
                order.Notes = request.Notes;

            if (isAdmin && request.TrackingNumber != null)
                order.TrackingNumber = request.TrackingNumber;

            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            return await GetOrderDtoByIdAsync(order.Id);
        }

        public async Task<bool> DeleteOrderAsync(int id, int userId, bool isAdmin)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return false;

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this order.");

            if (!isAdmin && order.OrderStatus != OrderStatus.WaitingPayment && order.OrderStatus != OrderStatus.Cancelled)
                throw new InvalidOperationException("Only pending or cancelled orders can be deleted by users.");

            _unitOfWork.Orders.Delete(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int id, int changedByUserId, UpdateOrderStatusDto request, bool isAdmin)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only administrators can update order status.");

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var oldStatus = order.OrderStatus;
            order.OrderStatus = request.OrderStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            await AddStatusHistoryAsync(order.Id, oldStatus, request.OrderStatus, changedByUserId, request.Notes);

            return await GetOrderDtoByIdAsync(order.Id);
        }

        public async Task<IEnumerable<OrderStatusHistoryDto>> GetOrderTimelineAsync(int id, int? userId, bool isAdmin)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this order timeline.");

            var history = await _unitOfWork.OrderStatusHistories.GetQueryable()
                .Include(h => h.ChangedByUser)
                .Where(h => h.OrderId == id)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderStatusHistoryDto>>(history);
        }

        public async Task LinkPaymentToOrderAsync(int orderId, int paymentId, int userId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to link payment to this order.");

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            if (payment.UserId != userId)
                throw new UnauthorizedAccessException("Payment does not belong to the current user.");

            var oldStatus = order.OrderStatus;
            order.PaymentId = paymentId;
            order.OrderStatus = OrderStatus.PaymentSubmitted;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            await AddStatusHistoryAsync(order.Id, oldStatus, OrderStatus.PaymentSubmitted, userId, "Payment linked to order.");
        }

        internal static async Task UpdateOrderStatusFromPaymentAsync(IUnitOfWork unitOfWork, Payment payment, int reviewerUserId, PaymentStatus paymentStatus)
        {
            if (payment.Type != PaymentType.PrintOrder)
                return;

            var order = await unitOfWork.Orders.GetQueryable()
                .FirstOrDefaultAsync(o => o.Id == payment.RelatedRecordId || o.PaymentId == payment.Id);

            if (order == null)
                return;

            var oldStatus = order.OrderStatus;
            OrderStatus newStatus = paymentStatus switch
            {
                PaymentStatus.Paid => OrderStatus.Approved,
                PaymentStatus.Rejected => OrderStatus.Rejected,
                PaymentStatus.Refunded => OrderStatus.Cancelled,
                _ => OrderStatus.UnderReview
            };

            if (order.PaymentId == null)
                order.PaymentId = payment.Id;

            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Orders.Update(order);
            await unitOfWork.CompleteAsync();

            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId = reviewerUserId,
                Notes = $"Payment status changed to {paymentStatus}.",
                CreatedAt = DateTime.UtcNow
            };
            await unitOfWork.OrderStatusHistories.AddAsync(history);
            await unitOfWork.CompleteAsync();
        }

        private async Task ValidatePrintOrderAsync(int userId, OrderType orderType, int relatedRecordId)
        {
            if (orderType == OrderType.CertificatePrint)
            {
                var certificate = await _unitOfWork.Certificates.GetQueryable()
                    .FirstOrDefaultAsync(c => c.Id == relatedRecordId && c.UserId == userId);

                if (certificate == null)
                    throw new KeyNotFoundException("Certificate not found.");

                if (certificate.Type == CertificateType.Training && certificate.RelatedRecordId.HasValue)
                {
                    var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                        .Include(e => e.Course)
                        .FirstOrDefaultAsync(e => e.CourseId == certificate.RelatedRecordId.Value && e.UserId == userId);

                    if (enrollment?.Course == null)
                        throw new InvalidOperationException("Course enrollment not found for this certificate.");

                    if (DateTime.UtcNow.Date <= enrollment.Course.EndDate.Date)
                        throw new InvalidOperationException("Certificate printing is allowed only after course completion.");
                }
            }
            else if (orderType == OrderType.AccreditationCardPrint)
            {
                var accreditation = await _unitOfWork.MediaAccreditations.GetQueryable()
                    .Include(a => a.MediaCard)
                    .FirstOrDefaultAsync(a => a.Id == relatedRecordId && a.UserId == userId);

                if (accreditation == null)
                    throw new KeyNotFoundException("Accreditation not found.");

                if (accreditation.Status != ApplicationStatus.Approved || accreditation.MediaCard == null)
                    throw new InvalidOperationException("Accreditation must be approved with an issued card before printing.");
            }
        }

        private IQueryable<Order> GetOrderQuery()
        {
            return _unitOfWork.Orders.GetQueryable()
                .Include(o => o.User)
                .Include(o => o.Payment);
        }

        private static IQueryable<Order> ApplyOrderSearch(IQueryable<Order> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            return query.Where(o =>
                o.OrderNumber.Contains(search) ||
                (o.TrackingNumber != null && o.TrackingNumber.Contains(search)) ||
                o.OrderStatus.ToString().Contains(search));
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            string orderNumber;
            do
            {
                orderNumber = $"GACAM-ORD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
            }
            while (await _unitOfWork.Orders.GetQueryable().AnyAsync(o => o.OrderNumber == orderNumber));

            return orderNumber;
        }

        private async Task AddStatusHistoryAsync(int orderId, OrderStatus? oldStatus, OrderStatus newStatus, int? userId, string? notes)
        {
            var history = new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId = userId,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OrderStatusHistories.AddAsync(history);
            await _unitOfWork.CompleteAsync();
        }

        private async Task<OrderDto> GetOrderDtoByIdAsync(int id)
        {
            var order = await GetOrderQuery().FirstAsync(o => o.Id == id);
            return _mapper.Map<OrderDto>(order);
        }

        private PagedResponse<OrderDto> MapOrderPage(PagedResponse<Order> paged)
        {
            return new PagedResponse<OrderDto>
            {
                Items = _mapper.Map<IEnumerable<OrderDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }
    }
}
