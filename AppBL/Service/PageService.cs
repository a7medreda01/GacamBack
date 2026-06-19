using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AppDAL.Entities;
namespace AppBL.Service
{
    public class PageService : IPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<PageDto>> GetAllPagesAsync(PagedRequestDto request)
        {
            IQueryable<Page> query = _unitOfWork.Pages.GetQueryable()
                .Include(p => p.UpdatedByUser);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(p =>
                    p.Slug.Contains(search) ||
                    p.TitleEn.Contains(search) ||
                    p.TitleAr.Contains(search));
            }

            query = query.OrderBy(p => p.Slug);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<PageDto>
            {
                Items = _mapper.Map<IEnumerable<PageDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<PageDto?> GetPageBySlugAsync(string slug)
        {
            var page = await _unitOfWork.Pages.GetQueryable()
                .Include(p => p.UpdatedByUser)
                .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower());

            return page == null ? null : _mapper.Map<PageDto>(page);
        }

        public async Task<PageDto> UpdatePageAsync(string slug, PageUpdateRequest request, int updatedByUserId)
        {
            var page = await _unitOfWork.Pages.GetQueryable()
                .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower());

            if (page == null)
                throw new KeyNotFoundException($"Page with slug '{slug}' not found.");

            _mapper.Map(request, page);
            page.UpdatedAt = DateTime.UtcNow;
            page.UpdatedByUserId = updatedByUserId;

            _unitOfWork.Pages.Update(page);
            await _unitOfWork.CompleteAsync();

            var updatedPage = await _unitOfWork.Pages.GetQueryable()
                .Include(p => p.UpdatedByUser)
                .FirstAsync(p => p.Id == page.Id);

            return _mapper.Map<PageDto>(updatedPage);
        }
    }
}
