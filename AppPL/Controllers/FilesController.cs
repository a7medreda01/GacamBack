using AppBL.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppPL.Controllers
{
    /// <summary>
    /// General-purpose file upload controller.
    /// Allows uploading images and documents to designated wwwroot/uploads subfolders.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileHelper _fileHelper;

        public FilesController(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }

        /// <summary>
        /// Upload any file to the specified subfolder under wwwroot/uploads/.
        /// Allowed folders: images, cvs, receipts, logos, documents
        /// </summary>
        [HttpPost("upload/{folder}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromRoute] string folder, IFormFile file)
        {
            var allowedFolders = new[] { "images", "cvs", "receipts", "logos", "documents", "news" };
            if (!allowedFolders.Contains(folder.ToLower()))
                return BadRequest(new { Message = $"Folder '{folder}' is not allowed. Allowed: {string.Join(", ", allowedFolders)}" });

            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided." });

            // 10 MB limit
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds the 10 MB limit." });

            var relativePath = await _fileHelper.UploadFileAsync(file, folder);
            var absoluteUrl  = _fileHelper.ResolveUrl(relativePath);

            return Ok(new
            {
                RelativePath = relativePath,
                AbsoluteUrl  = absoluteUrl,
                FileName     = Path.GetFileName(relativePath)
            });
        }
    }
}
