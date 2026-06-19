using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBL.DTOs
{
    public class FileVerifyResultDto
    {
        public bool IsValid { get; set; }
        public string DocumentType { get; set; } = string.Empty; // "Certificate" | "MediaCard" | "Unknown"
        public CertificateVerifyDto? Certificate { get; set; }
        public CardVerifyDto? Card { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
