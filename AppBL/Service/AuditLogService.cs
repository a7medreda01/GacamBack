using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AppBL.Service
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task LogAsync(int? userId, string? email, string action, string tableName, string? recordId, object? oldValues = null, object? newValues = null, string? ipAddress = null)
        {
            await _unitOfWork.AuditLogs.AddAsync(new AppDAL.Entities.AuditLog
            {
                UserId = userId,
                UserEmail = email,
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResponse<AuditLogDto>> GetAllLogsAsync(PagedRequestDto request)
        {
            var query = _unitOfWork.AuditLogs.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(l =>
                    l.Action.Contains(search) ||
                    l.TableName.Contains(search) ||
                    (l.UserEmail != null && l.UserEmail.Contains(search)) ||
                    (l.RecordId != null && l.RecordId.Contains(search)));
            }

            query = query.OrderByDescending(l => l.Timestamp);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<AuditLogDto>
            {
                Items = _mapper.Map<IEnumerable<AuditLogDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }
    }
}
