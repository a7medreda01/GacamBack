using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace AppBL.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileHelper _fileHelper;

        static CertificateService()
        {
            // Set QuestPDF Community License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public CertificateService(IUnitOfWork unitOfWork, IMapper mapper, IFileHelper fileHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileHelper = fileHelper;
        }

        public async Task<CertificateDto> RequestCertificateAsync(int userId, CertificateRequestDto request)
        {
            // Verify eligibility
            if (request.Type == CertificateType.Training)
            {
                if (!request.RelatedRecordId.HasValue)
                    throw new ArgumentException("CourseId is required for Training Certificate.");

                var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                    .FirstOrDefaultAsync(ce => ce.CourseId == request.RelatedRecordId.Value && ce.UserId == userId && ce.Status == EnrollmentStatus.Approved);

                if (enrollment == null)
                    throw new InvalidOperationException("You must be enrolled and approved in the course to request a certificate.");
            }
            else if (request.Type == CertificateType.Volunteer)
            {
                if (!request.RelatedRecordId.HasValue)
                    throw new ArgumentException("VolunteerId is required for Volunteer Certificate.");

                var volunteer = await _unitOfWork.Volunteers.GetQueryable()
                    .FirstOrDefaultAsync(v => v.Id == request.RelatedRecordId.Value && v.UserId == userId && v.Status == ApplicationStatus.Approved);

                if (volunteer == null)
                    throw new InvalidOperationException("You must be an approved volunteer to request a certificate.");
            }

            // Generate unique certificate number
            string certificateNumber = $"GACAM-CERT-{DateTime.UtcNow.Year}-{new Random().Next(10000, 99999)}";
            while (await _unitOfWork.Certificates.GetQueryable().AnyAsync(c => c.CertificateNumber == certificateNumber))
            {
                certificateNumber = $"GACAM-CERT-{DateTime.UtcNow.Year}-{new Random().Next(10000, 99999)}";
            }

            // Create certificate record — PDF is generated on demand, not saved to disk
            var certificate = new Certificate
            {
                UserId = userId,
                Type = request.Type,
                RelatedRecordId = request.RelatedRecordId,
                FullNameOnCertificate = request.FullNameOnCertificate,
                CertificateNumber = certificateNumber,
                IssuedAt = DateTime.UtcNow,
                QrCodeData = $"{_fileHelper.GetFrontendUrl()}/verify-certificate/{certificateNumber}",
                PdfUrl = string.Empty
            };

            await _unitOfWork.Certificates.AddAsync(certificate);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CertificateDto>(certificate);
        }

        public async Task<PagedResponse<CertificateDto>> GetUserCertificatesAsync(int userId, PagedRequestDto request)
        {
            var query = _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt);

            var paged = await AppBL.Helper.PaginationHelper.ToPagedResponseAsync(query, request);
            return MapCertificatePage(paged);
        }

        public async Task<PagedResponse<CertificateDto>> GetAllCertificatesAsync(PagedRequestDto request)
        {
            IQueryable<Certificate> query = _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(c =>
                    c.CertificateNumber.Contains(search) ||
                    c.FullNameOnCertificate.Contains(search));
            }

            query = query.OrderByDescending(c => c.IssuedAt);

            var paged = await AppBL.Helper.PaginationHelper.ToPagedResponseAsync(query, request);
            return MapCertificatePage(paged);
        }

        public async Task<CertificateDto?> GetCertificateByIdAsync(int id)
        {
            var certificate = await _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
                return null;

            return _mapper.Map<CertificateDto>(certificate);
        }

        // ─── Core PDF Builder ──────────────────────────────────────────────────

        /// <summary>
        /// Builds the certificate PDF entirely in memory using data from the database.
        /// No file is written to disk. Returns raw PDF bytes.
        /// </summary>
        public async Task<byte[]> GenerateCertificatePdfBytesAsync(int certificateId)
        {
            var certificate = await _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            if (certificate == null)
                throw new KeyNotFoundException("Certificate not found.");

            // 1. Build QR Code bytes in memory (no file write)
            string qrText = certificate.QrCodeData;
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(
                20,
                new byte[] { 0, 63, 74 },      // ← لون الـ QR = #003F4A (تيلي داكن)
                new byte[] { 255, 255, 255 }    // ← الخلفية = أبيض
            );

            // 2. Fetch configuration and design settings
            var setting = await _unitOfWork.Settings.GetQueryable().FirstOrDefaultAsync();
            var design = await _unitOfWork.CertificateDesigns.GetQueryable().FirstOrDefaultAsync();

            string primaryColor = design?.PrimaryColor ?? "#003F4A";
            string secondaryColor = design?.SecondaryColor ?? "#a97542";
            string borderColor = design?.BorderColor ?? primaryColor;
            float borderWidth = design?.BorderWidth ?? 10f;

            string headerEn = design?.HeaderTextEn ?? "GULF & ARAB GENERAL COMMISSION FOR AUDIOVISUAL MEDIA";
            string titleEn = design?.TitleEn ?? "CERTIFICATE OF TRAINING";

            // 3. Load optional images from disk (logo, signature, background)
            byte[]? logoBytes = null;
            byte[]? signatureBytes = null;
            byte[]? backgroundBytes = null;

            if (design == null || design.ShowLogo)
            {
                if (!string.IsNullOrEmpty(setting?.LogoUrl))
                {
                    string logoPath = Path.Combine("wwwroot", setting.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(logoPath))
                        logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
                }
            }

            if (!string.IsNullOrEmpty(design?.SignatureImageUrl))
            {
                string sigPath = Path.Combine("wwwroot", design.SignatureImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(sigPath))
                    signatureBytes = await System.IO.File.ReadAllBytesAsync(sigPath);
            }

            if (!string.IsNullOrEmpty(design?.BackgroundImageUrl))
            {
                string bgPath = Path.Combine("wwwroot", design.BackgroundImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(bgPath))
                    backgroundBytes = await System.IO.File.ReadAllBytesAsync(bgPath);
            }

            // 4. Fetch course title and duration (training certificates)
            string durationText = "—";
            string courseTitle = string.Empty;

            if (certificate.Type == CertificateType.Training && certificate.RelatedRecordId.HasValue)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(certificate.RelatedRecordId.Value);
                if (course != null)
                {
                    durationText = FormatCourseDuration(course);
                    courseTitle = course.TitleEn;
                }
            }
            else if (certificate.Type == CertificateType.Volunteer)
            {
                courseTitle = "Volunteering Service";
            }

            string completionDate = certificate.IssuedAt.ToString("dd MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
            bool useBackgroundTemplate = backgroundBytes != null;

            // 5. Generate PDF in memory
            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    if (useBackgroundTemplate)
                    {
                        page.Margin(0);
                        page.Background().Image(backgroundBytes!).FitArea();
                        page.Content().Element(c =>
                            ComposeBackgroundOverlay(
                                c, certificate, completionDate, durationText,
                                courseTitle, signatureBytes, qrBytes, primaryColor));
                    }
                    else
                    {
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.Foreground().Border(borderWidth).BorderColor(borderColor);
                        page.Content().Element(c =>
                            ComposeClassicLayout(c, certificate, completionDate, durationText, design, logoBytes, signatureBytes, qrBytes,
                                primaryColor, secondaryColor, headerEn, titleEn));
                    }
                });
            }).GeneratePdf();

            return pdfBytes;
        }

        /// <summary>
        /// Overlays dynamic certificate data on the GACAM background template.
        /// Positions are tuned for A4 landscape (842×595 pt) to align with the uploaded artwork.
        /// 
        /// Vertical anchors (measured from top of page):
        ///   Recipient Name        ≈ 322 pt  (57% of 595)
        ///   Training Program      ≈ +18 pt below name
        ///   Date / Duration / No  ≈ +28 pt below title
        ///   Signature / QR        ≈ +38 pt below info row
        /// </summary>
        private static void ComposeBackgroundOverlay(
            IContainer container,
            Certificate certificate,
            string completionDate,
            string durationText,
            string courseTitle,
            byte[]? signatureBytes,
            byte[] qrBytes,
            string primaryColor)
        {
            container.Column(column =>
            {
                // ── Spacer to Recipient Name ────────────────────────────────────
                column.Item().Height(315);

                // ── Recipient Name ──────────────────────────────────────────────
                column.Item()
                    .PaddingHorizontal(80)
                    .AlignCenter()
                    .Text(certificate.FullNameOnCertificate)
                    .FontSize(30).Bold()
                    .FontColor(primaryColor);

                // ── Gold underline ──────────────────────────────────────────────
                column.Item()
                    .PaddingTop(6).PaddingHorizontal(100)
                    .Height(1.5f)
                    .Background("#C9A96B");

                // ── "HAS SUCCESSFULLY..." ───────────────────────────────────────
                column.Item()
                    .PaddingTop(8)
                    .AlignCenter()
                    .Text("HAS SUCCESSFULLY COMPLETED THE TRAINING PROGRAM")
                    .FontSize(11).Bold()
                    .FontColor("#444444");

                // ── Training Program Title ──────────────────────────────────────
                if (!string.IsNullOrEmpty(courseTitle))
                {
                    column.Item()
                        .PaddingTop(6).PaddingHorizontal(60)
                        .Row(row =>
                        {
                            row.RelativeItem().AlignRight().PaddingRight(6)
                                .Text("——◆")
                                .FontSize(12).FontColor("#C9A96B");

                            row.AutoItem()
                                .AlignCenter()
                                .Text(courseTitle.ToUpper())
                                .FontSize(15).Bold()
                                .FontColor(primaryColor);

                            row.RelativeItem().AlignLeft().PaddingLeft(6)
                                .Text("◆——")
                                .FontSize(12).FontColor("#C9A96B");
                        });
                }

                // ── Gap للـ info row ────────────────────────────────────────────
                column.Item().Height(5);

                // ── Info Row ────────────────────────────────────────────────────
                column.Item().PaddingHorizontal(55).Row(row =>
                {
                    // Date
                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text("DATE OF COMPLETION")
                            .FontSize(6).Bold().FontColor("#777777");
                        c.Item().PaddingTop(2).AlignCenter()
                            .Text(completionDate.ToUpper())
                            .FontSize(9).Bold()
                            .FontColor("#C9A96B");
                    });

                    row.ConstantItem(1).Background("#C9A96B");

                    // Duration
                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text("DURATION")
                            .FontSize(6).Bold().FontColor("#777777");
                        c.Item().PaddingTop(2).AlignCenter()
                            .Text(durationText.ToUpper())
                            .FontSize(9).Bold()
                            .FontColor("#C9A96B");
                    });

                    row.ConstantItem(1).Background("#C9A96B");

                    // Certificate No
                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text("CERTIFICATE NO.")
                            .FontSize(6).Bold().FontColor("#777777");
                        c.Item().PaddingTop(2).AlignCenter()
                            .Text(certificate.CertificateNumber)
                            .FontSize(8).Bold()
                            .FontColor("#C9A96B");
                    });
                });

                // ── Gap قبل Bottom row ──────────────────────────────────────────
                column.Item().Height(34);

                // ── Signature (يسار) ────────────────────────────────────────────
                column.Item().Height(10);
                column.Item().PaddingLeft(35).Row(row =>
                {
                    row.ConstantItem(170).Column(c =>
                    {
                        if (signatureBytes != null)
                            c.Item().Height(45).Width(130).Image(signatureBytes);
                        c.Item().Width(130).Height(1).Background("#444444");
                        c.Item().PaddingTop(3)
                            .Text("J. AL MAJED")
                            .FontSize(9).Bold().FontColor(primaryColor);
                        c.Item()
                            .Text("DIRECTOR GENERAL")
                            .FontSize(7).Italic().FontColor("#666666");
                    });
                });

                // ── QR + Dates (يمين) ───────────────────────────────────────────
                column.Item().PaddingTop(-55).PaddingRight(120).AlignRight().Row(r =>
                {
                    r.ConstantItem(50).Height(50)
                        .Border(1.5f).BorderColor("#C9A96B")  // ← هنا
                        .Image(qrBytes);

                    r.AutoItem().PaddingLeft(8).Column(c =>
                    {
                        c.Item()
                            .Text("ISSUE DATE")
                            .FontSize(6.5f).Bold().FontColor("#777777");
                        c.Item()
                            .Text(certificate.IssuedAt.ToString("dd / MM / yyyy"))
                            .FontSize(8f).Bold().FontColor("#C9A96B");
                    });
                });
            });
        }
        /// <summary>Fallback layout when no background image is configured.</summary>
        private static void ComposeClassicLayout(
            IContainer container,
            Certificate certificate,
            string completionDate,
            string durationText,
            CertificateDesign? design,
            byte[]? logoBytes,
            byte[]? signatureBytes,
            byte[] qrBytes,
            string primaryColor,
            string secondaryColor,
            string headerEn,
            string titleEn)
        {
            container.Padding(2, Unit.Centimetre).Column(column =>
            {
                if (logoBytes != null)
                {
                    column.Item().AlignCenter().PaddingBottom(0.2f, Unit.Centimetre)
                        .Height(design?.LogoHeight ?? 60f).Image(logoBytes);
                }

                column.Item().Text(headerEn)
                    .FontSize(14).Bold().FontColor(primaryColor).AlignCenter();

                column.Item().PaddingTop(0.5f, Unit.Centimetre).Text(titleEn)
                    .FontSize(22).Bold().FontColor(primaryColor).AlignCenter();

                column.Item().PaddingTop(0.8f, Unit.Centimetre)
                    .Text("This is to certify that")
                    .FontSize(11).Italic().AlignCenter();

                column.Item().PaddingVertical(0.4f, Unit.Centimetre)
                    .Text(certificate.FullNameOnCertificate)
                    .FontSize(24).Bold().FontColor(primaryColor).AlignCenter();

                column.Item().Text("has successfully completed the training program")
                    .FontSize(11).AlignCenter();

                column.Item().PaddingTop(1f, Unit.Centimetre).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Date of Completion: {completionDate}").FontSize(9);
                        c.Item().Text($"Duration: {durationText}").FontSize(9);
                        c.Item().Text($"Certificate No: {certificate.CertificateNumber}").FontSize(9);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text(design?.SignatoryName ?? "Executive Director")
                            .FontSize(10).Bold().FontColor(primaryColor);
                        c.Item().AlignCenter().Text(design?.SignatoryTitleEn ?? "GACAM Administration")
                            .FontSize(8).Italic();
                        if (signatureBytes != null)
                            c.Item().AlignCenter().Height(30f).Image(signatureBytes);
                    });

                    row.ConstantItem(60).Image(qrBytes);
                });
            });
        }

        private static string FormatCourseDuration(Course course)
        {
            int days = (int)Math.Max(1, (course.EndDate.Date - course.StartDate.Date).TotalDays + 1);
            return days == 1 ? "1 Day" : $"{days} Days";
        }

        /// <summary>
        /// Legacy method — generates PDF bytes (no file saved) and returns the DTO.
        /// </summary>
        public async Task<CertificateDto> GenerateCertificatePdfAsync(int certificateId)
        {
            // Just generate bytes (not stored) to keep backward-compat API
            await GenerateCertificatePdfBytesAsync(certificateId);

            var certificate = await _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            return _mapper.Map<CertificateDto>(certificate!);
        }

        // ─── Verification ──────────────────────────────────────────────────────

        public async Task<CertificateVerifyDto> VerifyCertificateAsync(string certificateNumber)
        {
            if (string.IsNullOrEmpty(certificateNumber))
                return new CertificateVerifyDto { IsValid = false };

            // Extract certificate number from URL if scanned from QR code
            if (certificateNumber.Contains("/"))
                certificateNumber = certificateNumber.Substring(certificateNumber.LastIndexOf('/') + 1);

            var certificate = await _unitOfWork.Certificates.GetQueryable()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            if (certificate == null)
                return new CertificateVerifyDto { IsValid = false };

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

            return new CertificateVerifyDto
            {
                IsValid = true,
                CertificateNumber = certificate.CertificateNumber,
                FullNameOnCertificate = certificate.FullNameOnCertificate,
                Type = certificate.Type.ToString(),
                RelatedItemTitle = relatedTitle,
                IssuedAt = certificate.IssuedAt,
            };
        }

        public async Task<CertificateVerifyDto> VerifyCertificateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new CertificateVerifyDto { IsValid = false };

            string extension = Path.GetExtension(file.FileName).ToLower();
            string? qrText = null;

            if (extension == ".pdf")
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    using var document = UglyToad.PdfPig.PdfDocument.Open(stream);

                    foreach (var page in document.GetPages())
                    {
                        foreach (var pdfImage in page.GetImages())
                        {
                            byte[]? imageBytes = null;

                            if (pdfImage.TryGetPng(out var pngBytes))
                                imageBytes = pngBytes;
                            else
                                imageBytes = pdfImage.RawBytes.ToArray();

                            if (imageBytes == null || imageBytes.Length == 0)
                                continue;

                            qrText = DecodeQrFromBytes(imageBytes);

                            if (!string.IsNullOrEmpty(qrText))
                                break;
                        }

                        if (!string.IsNullOrEmpty(qrText))
                            break;
                    }
                }
                catch
                {
                    // fallback
                }
            }
            else if (extension is ".png" or ".jpg" or ".jpeg" or ".bmp")
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    qrText = DecodeQrFromBytes(ms.ToArray());
                }
                catch
                {
                    // fallback
                }
            }

            if (string.IsNullOrEmpty(qrText))
                return new CertificateVerifyDto { IsValid = false };

            // ─────────────────────────────────────────────
            // 1) Try Certificate verification first
            // ─────────────────────────────────────────────
            var certificateResult = await VerifyCertificateAsync(qrText);
            if (certificateResult.IsValid)
                return certificateResult;

            // ─────────────────────────────────────────────
            // 2) Try Media Card verification
            // ─────────────────────────────────────────────
            var card = await _unitOfWork.MediaCards.GetQueryable()
                .Include(mc => mc.Accreditation)
                    .ThenInclude(a => a.User)
                .Include(mc => mc.Accreditation)
                    .ThenInclude(a => a.AccreditationCategory)
                .FirstOrDefaultAsync(mc =>
                    mc.CardNumber == qrText ||
                    mc.QrCodeData == qrText);

            if (card == null)
                return new CertificateVerifyDto { IsValid = false };

            if (card.Status == CardStatus.Active && DateTime.UtcNow > card.ExpiresAt)
            {
                card.Status = CardStatus.Expired;
                _unitOfWork.MediaCards.Update(card);
                await _unitOfWork.CompleteAsync();
            }

            return new CertificateVerifyDto
            {
                IsValid = card.Status == CardStatus.Active,
                CertificateNumber = card.CardNumber,
                FullNameOnCertificate = card.Accreditation.User.FullName,
                Type = "MediaCard",
                RelatedItemTitle = card.Accreditation.AccreditationCategory.NameEn,
                IssuedAt = card.IssuedAt,
                ExpiredAt = card.ExpiresAt,
                IsExpired = DateTime.UtcNow > card.ExpiresAt
            };
        }
        private string? DecodeQrFromBytes(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                using var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms);
                var reader = new ZXing.Windows.Compatibility.BarcodeReader();
                var result = reader.Decode(bitmap);
                return result?.Text;
            }
            catch
            {
                return null;
            }
        }

        private PagedResponse<CertificateDto> MapCertificatePage(PagedResponse<Certificate> paged)
        {
            return new PagedResponse<CertificateDto>
            {
                Items = _mapper.Map<IEnumerable<CertificateDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }
    }
}