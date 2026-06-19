using AppBL.DTOs;

namespace AppBL.IService
{
    public interface IAuditLogService
    {
        Task LogAsync(int? userId, string? email, string action, string tableName, string? recordId, object? oldValues = null, object? newValues = null, string? ipAddress = null);
        Task<PagedResponse<AuditLogDto>> GetAllLogsAsync(PagedRequestDto request);
    }
}
