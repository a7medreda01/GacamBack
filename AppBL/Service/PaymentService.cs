using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto> SubmitPaymentAsync(int userId, PaymentSubmitRequest request)
        {
            var isDuplicateRef = await _unitOfWork.Payments.GetQueryable()
                .AnyAsync(p => p.ReferenceNumber == request.ReferenceNumber);

            if (isDuplicateRef)
                throw new InvalidOperationException("This reference number has already been submitted.");

            var payment = _mapper.Map<Payment>(request);
            payment.UserId = userId;
            payment.Status = PaymentStatus.PendingVerification;
            payment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            if (request.Type == PaymentType.Course && request.RelatedRecordId > 0)
            {
                var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                    .FirstOrDefaultAsync(ce => ce.Id == request.RelatedRecordId && ce.UserId == userId);

                if (enrollment != null)
                {
                    enrollment.PaymentId = payment.Id;
                    _unitOfWork.CourseEnrollments.Update(enrollment);
                    await _unitOfWork.CompleteAsync();
                }
            }
            else if (request.Type == PaymentType.PrintOrder && request.RelatedRecordId > 0)
            {
                await OrderService.UpdateOrderStatusFromPaymentAsync(_unitOfWork, payment, userId, PaymentStatus.PendingVerification);

                var order = await _unitOfWork.Orders.GetQueryable()
                    .FirstOrDefaultAsync(o => o.Id == request.RelatedRecordId && o.UserId == userId);

                if (order != null)
                {
                    order.PaymentId = payment.Id;
                    order.OrderStatus = OrderStatus.PaymentSubmitted;
                    order.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.CompleteAsync();
                }
            }

            var savedPayment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .FirstAsync(p => p.Id == payment.Id);

            return _mapper.Map<PaymentDto>(savedPayment);
        }

        public async Task<PagedResponse<PaymentDto>> GetAllPaymentsAsync(PagedRequestDto request, PaymentStatus? status = null)
        {
            var query = _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .Include(p => p.VerifiedByUser)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(p =>
                    p.SenderName.Contains(search) ||
                    p.ReferenceNumber.Contains(search));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var paged = await AppBL.Helper.PaginationHelper.ToPagedResponseAsync(query, request);
            return MapPaymentPage(paged);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .Include(p => p.VerifiedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PagedResponse<PaymentDto>> GetUserPaymentsAsync(int userId, PagedRequestDto request)
        {
            var query = _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .Include(p => p.VerifiedByUser)
                .Where(p => p.UserId == userId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(p =>
                    p.SenderName.Contains(search) ||
                    p.ReferenceNumber.Contains(search));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var paged = await AppBL.Helper.PaginationHelper.ToPagedResponseAsync(query, request);
            return MapPaymentPage(paged);
        }

        public async Task<PaymentDto> ReviewPaymentAsync(int paymentId, int reviewerUserId, PaymentReviewRequest request)
        {
            var payment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new KeyNotFoundException("Payment record not found.");

            payment.Status = request.Status;
            payment.VerifiedAt = DateTime.UtcNow;
            payment.VerifiedByUserId = reviewerUserId;
            payment.AdminNotes = request.AdminNotes;

            if (request.Status == PaymentStatus.Paid)
            {
                if (payment.Type == PaymentType.Accreditation)
                {
                    await HandleAccreditationPaymentApprovedAsync(payment, reviewerUserId);
                }
                else if (payment.Type == PaymentType.Course)
                {
                    var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                        .FirstOrDefaultAsync(ce => ce.Id == payment.RelatedRecordId);
                    if (enrollment != null)
                    {
                        enrollment.Status = EnrollmentStatus.Approved;
                        enrollment.PaymentId = payment.Id;
                        _unitOfWork.CourseEnrollments.Update(enrollment);
                    }
                }
                else if (payment.Type == PaymentType.PrintOrder)
                {
                    await OrderService.UpdateOrderStatusFromPaymentAsync(_unitOfWork, payment, reviewerUserId, PaymentStatus.Paid);
                }
            }
            else if (request.Status == PaymentStatus.Rejected || request.Status == PaymentStatus.Refunded)
            {
                if (payment.Type == PaymentType.Accreditation)
                {
                    var accreditation = await _unitOfWork.MediaAccreditations.GetQueryable()
                        .FirstOrDefaultAsync(ma => ma.Id == payment.RelatedRecordId);
                    if (accreditation != null)
                    {
                        accreditation.Status = ApplicationStatus.Rejected;
                        accreditation.CheckedAt = DateTime.UtcNow;
                        accreditation.CheckedByUserId = reviewerUserId;
                        _unitOfWork.MediaAccreditations.Update(accreditation);
                    }
                }
                else if (payment.Type == PaymentType.Course)
                {
                    var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                        .FirstOrDefaultAsync(ce => ce.Id == payment.RelatedRecordId);
                    if (enrollment != null)
                    {
                        enrollment.Status = EnrollmentStatus.Rejected;
                        _unitOfWork.CourseEnrollments.Update(enrollment);
                    }
                }
                else if (payment.Type == PaymentType.PrintOrder)
                {
                    await OrderService.UpdateOrderStatusFromPaymentAsync(_unitOfWork, payment, reviewerUserId, request.Status);
                }
            }
            else if (payment.Type == PaymentType.PrintOrder)
            {
                await OrderService.UpdateOrderStatusFromPaymentAsync(_unitOfWork, payment, reviewerUserId, request.Status);
            }

            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.CompleteAsync();

            var updatedPayment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .Include(p => p.VerifiedByUser)
                .FirstAsync(p => p.Id == paymentId);

            return _mapper.Map<PaymentDto>(updatedPayment);
        }

        private async Task HandleAccreditationPaymentApprovedAsync(Payment payment, int reviewerUserId)
        {
            var accreditation = await _unitOfWork.MediaAccreditations.GetQueryable()
                .FirstOrDefaultAsync(ma => ma.Id == payment.RelatedRecordId);
            if (accreditation == null)
                return;

            accreditation.Status = ApplicationStatus.Approved;
            accreditation.CheckedAt = DateTime.UtcNow;
            accreditation.CheckedByUserId = reviewerUserId;

            string cardNumber = $"GACAM-ACC-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
            while (await _unitOfWork.MediaCards.GetQueryable().AnyAsync(mc => mc.CardNumber == cardNumber))
            {
                cardNumber = $"GACAM-ACC-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
            }

            string qrCodeText = $"https://gacam.media/verify/card/{cardNumber}";

            var mediaCard = new MediaCard
            {
                AccreditationId = accreditation.Id,
                CardNumber = cardNumber,
                QrCodeData = qrCodeText,
                Status = CardStatus.Active,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1)
            };

            await _unitOfWork.MediaCards.AddAsync(mediaCard);
            _unitOfWork.MediaAccreditations.Update(accreditation);
        }

        private PagedResponse<PaymentDto> MapPaymentPage(PagedResponse<Payment> paged)
        {
            return new PagedResponse<PaymentDto>
            {
                Items = _mapper.Map<IEnumerable<PaymentDto>>(paged.Items),
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
