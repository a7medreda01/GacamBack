using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface INewsService
    {
        Task<PagedResponse<NewsDto>> GetAllNewsAsync(PagedRequestDto request, NewsType? type = null);
        Task<NewsDto?> GetNewsByIdAsync(int id);
        Task<NewsDto> CreateNewsAsync(NewsCreateRequest request);
        Task<NewsDto> UpdateNewsAsync(int id, NewsUpdateRequest request);
        Task<bool> DeleteNewsAsync(int id);
        Task<bool> IncrementViewCountAsync(int id);
    }
}
