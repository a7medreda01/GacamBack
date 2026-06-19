namespace AppBL.IService
{
    public interface IReportService
    {
        Task<byte[]> GeneratePaymentsReportAsync();
        Task<byte[]> GenerateAuditLogsReportAsync();
        Task<byte[]> GenerateUsersReportAsync();
    }
}
