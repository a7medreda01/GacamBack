using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class MediaAccreditationService : IMediaAccreditationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileHelper _fileHelper;

        public MediaAccreditationService(IUnitOfWork unitOfWork, IMapper mapper, IFileHelper fileHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileHelper = fileHelper;
        }

        public async Task<AccreditationDto> ApplyAccreditationAsync(int userId, AccreditationApplyRequest request)
        {
            var categoryExists = await _unitOfWork.AccreditationCategories.GetQueryable()
                .AnyAsync(c => c.Id == request.AccreditationCategoryId && c.IsActive);

            if (!categoryExists)
                throw new InvalidOperationException("Invalid or inactive accreditation category.");

            var alreadyApplied = await _unitOfWork.MediaAccreditations.GetQueryable()
                .AnyAsync(ma => ma.UserId == userId && (ma.Status == ApplicationStatus.Pending || ma.Status == ApplicationStatus.Approved));

            if (alreadyApplied)
                throw new InvalidOperationException("You already have an active or pending accreditation application.");

            string documentUrl = string.Empty;
            if (request.Document != null && request.Document.Length > 0)
            {
                documentUrl = await _fileHelper.UploadFileAsync(request.Document, "documents");
            }

            var accreditation = new MediaAccreditation
            {
                UserId = userId,
                AccreditationCategoryId = request.AccreditationCategoryId,
                Status = ApplicationStatus.Pending,
                DocumentUrl = documentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MediaAccreditations.AddAsync(accreditation);
            await _unitOfWork.CompleteAsync();

            return await GetAccreditationDtoByIdAsync(accreditation.Id);
        }

        public async Task<PagedResponse<AccreditationDto>> GetAllAccreditationsAsync(PagedRequestDto request, ApplicationStatus? status = null)
        {
            var query = GetAccreditationQuery();

            if (status.HasValue)
                query = query.Where(ma => ma.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(ma =>
                    ma.User.FullName.Contains(search) ||
                    ma.User.Email.Contains(search) ||
                    ma.AccreditationCategory.NameEn.Contains(search) ||
                    ma.AccreditationCategory.NameAr.Contains(search) ||
                    ma.Status.ToString().Contains(search));
            }

            query = query.OrderByDescending(ma => ma.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<AccreditationDto>
            {
                Items = _mapper.Map<IEnumerable<AccreditationDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<AccreditationDto?> GetAccreditationByIdAsync(int id)
        {
            var accreditation = await GetAccreditationQuery()
                .FirstOrDefaultAsync(ma => ma.Id == id);

            return accreditation == null ? null : _mapper.Map<AccreditationDto>(accreditation);
        }

        public async Task<AccreditationDto?> GetAccreditationByUserIdAsync(int userId)
        {
            var accreditation = await GetAccreditationQuery()
                .FirstOrDefaultAsync(ma => ma.UserId == userId);

            return accreditation == null ? null : _mapper.Map<AccreditationDto>(accreditation);
        }

        public async Task<AccreditationDto> ReviewAccreditationAsync(int id, int reviewerUserId, AccreditationReviewRequest request)
        {
            var accreditation = await _unitOfWork.MediaAccreditations.GetQueryable()
                .Include(ma => ma.User)
                .Include(ma => ma.MediaCard)
                .FirstOrDefaultAsync(ma => ma.Id == id);

            if (accreditation == null)
                throw new KeyNotFoundException("Accreditation application not found.");

            accreditation.Status = request.Status;
            accreditation.CheckedAt = DateTime.UtcNow;
            accreditation.CheckedByUserId = reviewerUserId;

            if (request.Status == ApplicationStatus.Approved && accreditation.MediaCard == null)
            {
                string cardNumber = $"GACAM-ACC-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
                while (await _unitOfWork.MediaCards.GetQueryable().AnyAsync(mc => mc.CardNumber == cardNumber))
                {
                    cardNumber = $"GACAM-ACC-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
                }

                await _unitOfWork.MediaCards.AddAsync(new MediaCard
                {
                    AccreditationId = accreditation.Id,
                    CardNumber = cardNumber,
                    QrCodeData = $"{_fileHelper.GetFrontendUrl()}/verify-certificate/{cardNumber}",
                    Status = CardStatus.Active,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1)
                });
            }
            else if (request.Status == ApplicationStatus.Rejected || request.Status == ApplicationStatus.Refunded)
            {
                if (accreditation.MediaCard != null)
                {
                    accreditation.MediaCard.Status = CardStatus.Revoked;
                    _unitOfWork.MediaCards.Update(accreditation.MediaCard);
                }
            }

            _unitOfWork.MediaAccreditations.Update(accreditation);
            await _unitOfWork.CompleteAsync();

            return await GetAccreditationDtoByIdAsync(id);
        }

        public async Task<CardVerifyDto> VerifyCardAsync(string cardNumber)
        {
            var card = await GetCardQuery()
                .FirstOrDefaultAsync(mc => mc.CardNumber == cardNumber);

            return card == null
                ? new CardVerifyDto { IsValid = false }
                : await BuildCardVerifyDtoAsync(card);
        }

        public async Task<CardVerifyDto> VerifyCardByQrAsync(string qrCodeData)
        {
            var card = await GetCardQuery()
                .FirstOrDefaultAsync(mc => mc.QrCodeData == qrCodeData);

            return card == null
                ? new CardVerifyDto { IsValid = false }
                : await BuildCardVerifyDtoAsync(card);
        }

        private IQueryable<MediaAccreditation> GetAccreditationQuery()
        {
            return _unitOfWork.MediaAccreditations.GetQueryable()
                .Include(ma => ma.User)
                .Include(ma => ma.AccreditationCategory)
                .Include(ma => ma.CheckedByUser)
                .Include(ma => ma.MediaCard);
        }

        private IQueryable<MediaCard> GetCardQuery()
        {
            return _unitOfWork.MediaCards.GetQueryable()
                .Include(mc => mc.Accreditation)
                    .ThenInclude(ma => ma.User)
                .Include(mc => mc.Accreditation)
                    .ThenInclude(ma => ma.AccreditationCategory);
        }

        private async Task<AccreditationDto> GetAccreditationDtoByIdAsync(int id)
        {
            var accreditation = await GetAccreditationQuery().FirstAsync(ma => ma.Id == id);
            return _mapper.Map<AccreditationDto>(accreditation);
        }

        private async Task<CardVerifyDto> BuildCardVerifyDtoAsync(MediaCard card)
        {
            if (card.Status == CardStatus.Active && DateTime.UtcNow > card.ExpiresAt)
            {
                card.Status = CardStatus.Expired;
                _unitOfWork.MediaCards.Update(card);
                await _unitOfWork.CompleteAsync();
            }

            return new CardVerifyDto
            {
                IsValid = card.Status == CardStatus.Active,
                CardNumber = card.CardNumber,
                FullName = card.Accreditation.User.FullName,
                CategoryNameEn = card.Accreditation.AccreditationCategory.NameEn,
                CategoryNameAr = card.Accreditation.AccreditationCategory.NameAr,
                Status = card.Status.ToString(),
                IssuedAt = card.IssuedAt,
                ExpiresAt = card.ExpiresAt
            };
        }
    }
}
