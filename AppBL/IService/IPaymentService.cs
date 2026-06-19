using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IPaymentService
    {
        Task<PaymentDto> SubmitPaymentAsync(int userId, PaymentSubmitRequest request);
        Task<PagedResponse<PaymentDto>> GetAllPaymentsAsync(PagedRequestDto request, PaymentStatus? status = null);
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<PagedResponse<PaymentDto>> GetUserPaymentsAsync(int userId, PagedRequestDto request);
        Task<PaymentDto> ReviewPaymentAsync(int paymentId, int reviewerUserId, PaymentReviewRequest request);
    }
}
