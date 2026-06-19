using AppBL.DTOs;

namespace AppBL.IService
{
    public interface IAccreditationCategoryService
    {
        Task<PagedResponse<AccreditationCategoryDto>> GetAllAsync(PagedRequestDto request, bool? isActive = null);
        Task<AccreditationCategoryDto?> GetByIdAsync(int id);
        Task<AccreditationCategoryDto> CreateAsync(CreateAccreditationCategoryDto request);
        Task<AccreditationCategoryDto> UpdateAsync(int id, UpdateAccreditationCategoryDto request);
        Task<bool> DeleteAsync(int id);
    }
}
