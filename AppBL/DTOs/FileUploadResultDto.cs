using System;

namespace AppBL.DTOs
{
    /// <summary>
    /// DTO returned by the FilesController.Upload endpoint.
    /// Contains the relative path, absolute URL, and the stored file name.
    /// </summary>
    public class FileUploadResultDto
    {
        public string RelativePath { get; set; } = string.Empty;
        public string AbsoluteUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
