using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface ICertificateService
    {
        Task<CertificateDto> RequestCertificateAsync(int userId, CertificateRequestDto request);
        Task<PagedResponse<CertificateDto>> GetUserCertificatesAsync(int userId, PagedRequestDto request);
        Task<PagedResponse<CertificateDto>> GetAllCertificatesAsync(PagedRequestDto request);
        Task<CertificateDto?> GetCertificateByIdAsync(int id);
        Task<byte[]> GenerateCertificatePdfBytesAsync(int certificateId);
        Task<CertificateDto> GenerateCertificatePdfAsync(int certificateId);
        Task<CertificateVerifyDto> VerifyCertificateAsync(string certificateNumber);
        Task<CertificateVerifyDto> VerifyCertificateFileAsync(Microsoft.AspNetCore.Http.IFormFile file);
    }
}
