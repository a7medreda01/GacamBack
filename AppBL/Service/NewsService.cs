using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class NewsService : INewsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NewsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<NewsDto>> GetAllNewsAsync(PagedRequestDto request, NewsType? type = null)
        {
            var query = _unitOfWork.News.GetQueryable();

            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(n => n.TitleEn.Contains(search) || n.TitleAr.Contains(search));
            }

            query = query.OrderByDescending(n => n.PublishedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<NewsDto>
            {
                Items = _mapper.Map<IEnumerable<NewsDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<NewsDto?> GetNewsByIdAsync(int id)
        {
            var news = await _unitOfWork.News.GetByIdAsync(id);
            return news == null ? null : _mapper.Map<NewsDto>(news);
        }

        public async Task<NewsDto> CreateNewsAsync(NewsCreateRequest request)
        {
            var news = _mapper.Map<News>(request);
            news.PublishedAt = DateTime.UtcNow;
            news.ViewCount = 0;

            await _unitOfWork.News.AddAsync(news);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<NewsDto>(news);
        }

        public async Task<NewsDto> UpdateNewsAsync(int id, NewsUpdateRequest request)
        {
            var news = await _unitOfWork.News.GetByIdAsync(id);
            if (news == null)
                throw new KeyNotFoundException("News article not found.");

            _mapper.Map(request, news);
            _unitOfWork.News.Update(news);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<NewsDto>(news);
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            var news = await _unitOfWork.News.GetByIdAsync(id);
            if (news == null)
                return false;

            _unitOfWork.News.Delete(news);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var news = await _unitOfWork.News.GetByIdAsync(id);
            if (news == null)
                return false;

            news.ViewCount += 1;
            _unitOfWork.News.Update(news);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
