using AppBL.DTOs;
using Microsoft.AspNetCore.Http;

namespace AppBL.IService
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserDto> GetProfileAsync(int userId);
        Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<UserDto> UploadProfileImageAsync(int userId, IFormFile file);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<PagedResponse<UserDto>> GetAllUsersWithRolesAsync(PagedRequestDto request);
        Task<bool> AssignRoleAsync(int userId, string roleName);
        Task<bool> RemoveRoleAsync(int userId, string roleName);
    }
}
