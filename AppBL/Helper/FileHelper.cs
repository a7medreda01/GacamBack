using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AppBL.Helper
{
    public interface IFileHelper
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        string ResolveUrl(string? relativePath);
        void DeleteFile(string? relativePath);
        string GetBaseUrl();
        string GetFrontendUrl();
    }

    public class FileHelper : IFileHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public FileHelper(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            // Create uploads directory if not exists
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique file name
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative path
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public string ResolveUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath; // Fallback to relative

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}{relativePath.Replace("\\", "/")}";
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Fail silently or log
                }
            }
        }
        
        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
                return string.Empty;

            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }
        public string GetFrontendUrl()
        {
            return _configuration["Frontend:BaseUrl"]?.TrimEnd('/')
                   ?? string.Empty;
        }
    }
}
