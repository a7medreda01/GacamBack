using AppBL.DTOs;

namespace AppBL.IService
{
    public interface IPageService
    {
        Task<PagedResponse<PageDto>> GetAllPagesAsync(PagedRequestDto request);
        Task<PageDto?> GetPageBySlugAsync(string slug);
        Task<PageDto> UpdatePageAsync(string slug, PageUpdateRequest request, int updatedByUserId);
    }
}
