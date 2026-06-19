using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IPartnerService
    {
        Task<PagedResponse<PartnerDto>> GetAllPartnersAsync(PagedRequestDto request, PartnerCategory? category = null);
        Task<PartnerDto?> GetPartnerByIdAsync(int id);
        Task<PartnerDto> CreatePartnerAsync(PartnerCreateRequest request);
        Task<PartnerDto> UpdatePartnerAsync(int id, PartnerUpdateRequest request);
        Task<bool> DeletePartnerAsync(int id);
    }
}
