using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMapper _mapper;
        private readonly IFileHelper _fileHelper;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxProfileImageSizeBytes = 5 * 1024 * 1024;

        public AuthService(IUnitOfWork unitOfWork, IJwtHelper jwtHelper, IMapper mapper, IFileHelper fileHelper)
        {
            _unitOfWork = unitOfWork;
            _jwtHelper = jwtHelper;
            _mapper = mapper;
            _fileHelper = fileHelper;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _unitOfWork.Users.GetQueryable().AnyAsync(u => u.Email == request.Email);
            if (existingUser)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var defaultRole = await _unitOfWork.Roles.GetByIdAsync(3);
            if (defaultRole != null)
            {
                await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = 3 });
                await _unitOfWork.CompleteAsync();
            }

            var savedUser = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == user.Id);

            return MapUserDto(savedUser);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            return new LoginResponse
            {
                Token = _jwtHelper.GenerateToken(user, roles),
                User = MapUserDto(user)
            };
        }

        public async Task<UserDto> GetProfileAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return MapUserDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            var updatedUser = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == userId);

            return MapUserDto(updatedUser);
        }

        public async Task<UserDto> UploadProfileImageAsync(int userId, IFormFile file)
        {
            ValidateProfileImage(file);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                _fileHelper.DeleteFile(user.ProfileImageUrl);

            user.ProfileImageUrl = await _fileHelper.UploadFileAsync(file, "profiles");
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            var updatedUser = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == userId);

            return MapUserDto(updatedUser);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PagedResponse<UserDto>> GetAllUsersWithRolesAsync(PagedRequestDto request)
        {
            IQueryable<User> query = _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<UserDto>
            {
                Items = paged.Items.Select(MapUserDto),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var role = await _unitOfWork.Roles.GetQueryable().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new KeyNotFoundException($"Role '{roleName}' not found.");

            var alreadyHasRole = await _unitOfWork.UserRoles.GetQueryable()
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (alreadyHasRole)
                return true;

            await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = role.Id });
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(int userId, string roleName)
        {
            var role = await _unitOfWork.Roles.GetQueryable().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new KeyNotFoundException($"Role '{roleName}' not found.");

            var userRole = await _unitOfWork.UserRoles.GetQueryable()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole == null)
                return false;

            _unitOfWork.UserRoles.Delete(userRole);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        private UserDto MapUserDto(User user)
        {
            var dto = _mapper.Map<UserDto>(user);
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                dto.ProfileImageUrl = _fileHelper.ResolveUrl(user.ProfileImageUrl);
            return dto;
        }

        private static void ValidateProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Image file is required.");

            if (file.Length > MaxProfileImageSizeBytes)
                throw new ArgumentException("Profile image must not exceed 5 MB.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
                throw new ArgumentException("Allowed image formats: JPG, JPEG, PNG, WEBP.");
        }
    }
}
