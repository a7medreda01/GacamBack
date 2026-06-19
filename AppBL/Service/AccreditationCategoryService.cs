using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class AccreditationCategoryService : IAccreditationCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccreditationCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<AccreditationCategoryDto>> GetAllAsync(PagedRequestDto request, bool? isActive = null)
        {
            var query = _unitOfWork.AccreditationCategories.GetQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(c =>
                    c.NameEn.Contains(search) ||
                    c.NameAr.Contains(search));
            }

            query = query.OrderBy(c => c.DisplayOrder);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<AccreditationCategoryDto>
            {
                Items = _mapper.Map<IEnumerable<AccreditationCategoryDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<AccreditationCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.AccreditationCategories.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<AccreditationCategoryDto>(category);
        }

        public async Task<AccreditationCategoryDto> CreateAsync(CreateAccreditationCategoryDto request)
        {
            var category = _mapper.Map<AccreditationCategory>(request);
            category.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.AccreditationCategories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AccreditationCategoryDto>(category);
        }

        public async Task<AccreditationCategoryDto> UpdateAsync(int id, UpdateAccreditationCategoryDto request)
        {
            var category = await _unitOfWork.AccreditationCategories.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException("Accreditation category not found.");

            _mapper.Map(request, category);
            _unitOfWork.AccreditationCategories.Update(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AccreditationCategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.AccreditationCategories.GetByIdAsync(id);
            if (category == null)
                return false;

            var inUse = await _unitOfWork.MediaAccreditations.GetQueryable()
                .AnyAsync(ma => ma.AccreditationCategoryId == id);

            if (inUse)
                throw new InvalidOperationException("Cannot delete category that is assigned to accreditations.");

            _unitOfWork.AccreditationCategories.Delete(category);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
