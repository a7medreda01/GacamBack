using AppBL.DTOs;
using Microsoft.AspNetCore.Http;

namespace AppBL.IService
{
    public interface IVerificationService
    {
        Task<UnifiedVerificationResponseDto> VerifyAsync(string code);
        Task<UnifiedVerificationResponseDto> VerifyFileAsync(IFormFile file);
    }
}
