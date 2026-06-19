using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IMediaAccreditationService
    {
        Task<AccreditationDto> ApplyAccreditationAsync(int userId, AccreditationApplyRequest request);
        Task<PagedResponse<AccreditationDto>> GetAllAccreditationsAsync(PagedRequestDto request, ApplicationStatus? status = null);
        Task<AccreditationDto?> GetAccreditationByIdAsync(int id);
        Task<AccreditationDto?> GetAccreditationByUserIdAsync(int userId);
        Task<AccreditationDto> ReviewAccreditationAsync(int id, int reviewerUserId, AccreditationReviewRequest request);
        Task<CardVerifyDto> VerifyCardAsync(string cardNumber);
        Task<CardVerifyDto> VerifyCardByQrAsync(string qrCodeData);
    }
}
