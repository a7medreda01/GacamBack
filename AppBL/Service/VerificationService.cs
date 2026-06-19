using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace AppBL.Service
{
    public class VerificationService : IVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ─── Text / QR string verification ────────────────────────────────────

        public async Task<UnifiedVerificationResponseDto> VerifyAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Invalid("Record not found");

            code = ExtractCode(code);

            var certResult = await TryVerifyCertificateAsync(code);
            if (certResult != null) return certResult;

            var cardResult = await TryVerifyCardAsync(code);
            if (cardResult != null) return cardResult;

            return Invalid("Record not found");
        }

        // ─── File verification (PDF or image) ─────────────────────────────────

        public async Task<UnifiedVerificationResponseDto> VerifyFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Invalid("No file provided.");

            string extension = Path.GetExtension(file.FileName).ToLower();

            string? qrText = extension switch
            {
                ".pdf" => await ExtractQrFromPdfAsync(file),
                ".png" or ".jpg" or ".jpeg"
                      or ".bmp" or ".webp" => await ExtractQrFromImageAsync(file),
                _ => null
            };

            if (string.IsNullOrEmpty(qrText))
                return Invalid(
                    extension is ".pdf" or ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp"
                        ? "No QR code found in file."
                        : "Unsupported file type. Please upload a PDF or image.");

            return await VerifyAsync(qrText);
        }

        // ─── Private: certificate lookup ───────────────────────────────────────

        private async Task<UnifiedVerificationResponseDto?> TryVerifyCertificateAsync(string code)
        {
            var certificate = await _unitOfWork.Certificates.GetQueryable()
                .FirstOrDefaultAsync(c =>
                    c.CertificateNumber == code ||
                    c.QrCodeData == code ||
                    c.QrCodeData.Contains(code));

            if (certificate == null)
                return null;

            string relatedTitle = "GACAM Program";

            if (certificate.Type == CertificateType.Training && certificate.RelatedRecordId.HasValue)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(certificate.RelatedRecordId.Value);
                if (course != null)
                    relatedTitle = course.TitleEn;
            }
            else if (certificate.Type == CertificateType.Volunteer)
            {
                relatedTitle = "Volunteering Service";
            }

            return new UnifiedVerificationResponseDto
            {
                IsValid = true,
                Type = "Certificate",
                Data = new CertificateVerificationDataDto
                {
                    Id = certificate.Id,
                    CertificateNumber = certificate.CertificateNumber,
                    FullNameOnCertificate = certificate.FullNameOnCertificate,
                    Type = certificate.Type.ToString(),
                    RelatedItemTitle = relatedTitle,
                    IssuedAt = certificate.IssuedAt,
                    QrCodeData = certificate.QrCodeData
                }
            };
        }
        // ─── Private: media card lookup ────────────────────────────────────────

        private async Task<UnifiedVerificationResponseDto?> TryVerifyCardAsync(string code)
        {
            var card = await _unitOfWork.MediaCards.GetQueryable()
                .Include(mc => mc.Accreditation).ThenInclude(ma => ma.User)
                .Include(mc => mc.Accreditation).ThenInclude(ma => ma.AccreditationCategory)
                .FirstOrDefaultAsync(mc =>
                    mc.CardNumber == code ||
                    mc.QrCodeData == code ||
                    mc.QrCodeData.Contains(code));

            if (card == null) return null;

            if (card.Status == CardStatus.Active && DateTime.UtcNow > card.ExpiresAt)
            {
                card.Status = CardStatus.Expired;
                _unitOfWork.MediaCards.Update(card);
                await _unitOfWork.CompleteAsync();
            }

            bool isExpired = card.Status == CardStatus.Expired || DateTime.UtcNow > card.ExpiresAt;

            return new UnifiedVerificationResponseDto
            {
                IsValid = card.Status == CardStatus.Active,
                Type = "MediaCard",
                Data = new MediaCardVerificationDataDto
                {
                    Id = card.Id,
                    CardNumber = card.CardNumber,
                    FullName = card.Accreditation.User.FullName,
                    CategoryNameEn = card.Accreditation.AccreditationCategory.NameEn,
                    CategoryNameAr = card.Accreditation.AccreditationCategory.NameAr,
                    Status = card.Status.ToString(),
                    IssuedAt = card.IssuedAt,
                    ExpiresAt = card.ExpiresAt,
                    IsExpired = isExpired,
                    QrCodeData = card.QrCodeData
                }
            };
        }

        // ─── Private: QR extraction ────────────────────────────────────────────

        private static async Task<string?> ExtractQrFromPdfAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var pdf = PdfDocument.Open(stream);

                foreach (var page in pdf.GetPages())
                {
                    foreach (var pdfImage in page.GetImages())
                    {
                        byte[]? bytes = null;
                        if (pdfImage.TryGetPng(out var png)) bytes = png;
                        else bytes = pdfImage.RawBytes.ToArray();

                        if (bytes is not { Length: > 0 }) continue;

                        var text = DecodeQrFromBytes(bytes);
                        if (!string.IsNullOrEmpty(text)) return text;
                    }
                }
            }
            catch { /* fall through */ }

            return null;
        }

        private static async Task<string?> ExtractQrFromImageAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return DecodeQrFromBytes(ms.ToArray());
            }
            catch { return null; }
        }

        private static string? DecodeQrFromBytes(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                using var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms);
                var reader = new ZXing.Windows.Compatibility.BarcodeReader();
                return reader.Decode(bitmap)?.Text;
            }
            catch { return null; }
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private static string ExtractCode(string code)
        {
            if (code.Contains('/'))
                code = code[(code.LastIndexOf('/') + 1)..];
            return code.Trim();
        }

        private static UnifiedVerificationResponseDto Invalid(string message) =>
            new() { IsValid = false, Message = message };
    }
}