using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class PartnerService : IPartnerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PartnerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<PartnerDto>> GetAllPartnersAsync(PagedRequestDto request, PartnerCategory? category = null)
        {
            var query = _unitOfWork.Partners.GetQueryable();

            if (category.HasValue)
                query = query.Where(p => p.Category == category.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(p => p.NameEn.Contains(search) || p.NameAr.Contains(search));
            }

            query = query.OrderBy(p => p.DisplayOrder);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<PartnerDto>
            {
                Items = _mapper.Map<IEnumerable<PartnerDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<PartnerDto?> GetPartnerByIdAsync(int id)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            return partner == null ? null : _mapper.Map<PartnerDto>(partner);
        }

        public async Task<PartnerDto> CreatePartnerAsync(PartnerCreateRequest request)
        {
            var partner = _mapper.Map<Partner>(request);
            partner.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Partners.AddAsync(partner);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PartnerDto>(partner);
        }

        public async Task<PartnerDto> UpdatePartnerAsync(int id, PartnerUpdateRequest request)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                throw new KeyNotFoundException("Partner not found.");

            _mapper.Map(request, partner);
            _unitOfWork.Partners.Update(partner);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PartnerDto>(partner);
        }

        public async Task<bool> DeletePartnerAsync(int id)
        {
            var partner = await _unitOfWork.Partners.GetByIdAsync(id);
            if (partner == null)
                return false;

            _unitOfWork.Partners.Delete(partner);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
