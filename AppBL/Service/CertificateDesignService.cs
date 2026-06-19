using AppBL.IService;
using AppDAL.Context;
using AppDAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppBL.Service
{
    public class CertificateDesignService : ICertificateDesignService
    {
        private readonly AppDbContext _dbContext;

        public CertificateDesignService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CertificateDesign> GetAsync()
        {
            var design = await _dbContext.CertificateDesigns.FirstOrDefaultAsync();
            if (design == null)
            {
                design = new CertificateDesign
                {
                    PrimaryColor = "#003F4A",
                    SecondaryColor = "#C9A96B",
                    BorderColor = "#003F4A",
                    BorderWidth = 0f,
                    TitleEn = "CERTIFICATE OF TRAINING",
                    TitleAr = "شهادة تدريب",
                    HeaderTextEn = "GULF & ARAB GENERAL COMMISSION FOR AUDIOVISUAL MEDIA",
                    HeaderTextAr = "الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا",
                    SignatoryName = "Executive Director",
                    SignatoryTitleEn = "GACAM Administration",
                    SignatoryTitleAr = "إدارة الهيئة العامة للإعلام",
                    SignatureImageUrl = null,
                    BackgroundImageUrl = "/uploads/certificate-backgrounds/4228d373-9069-4d75-a20f-7adaa9a079cd_155171b4-82c7-4f54-90c2-223985998cbe.jfif",
                    ShowLogo = false,
                    LogoHeight = 60f
                };
                _dbContext.CertificateDesigns.Add(design);
                await _dbContext.SaveChangesAsync();
            }
            return design;
        }

        public async Task<CertificateDesign> UpdateAsync(CertificateDesign updated)
        {
            var existing = await _dbContext.CertificateDesigns.FirstOrDefaultAsync();
            if (existing == null)
            {
                _dbContext.CertificateDesigns.Add(updated);
                await _dbContext.SaveChangesAsync();
                return updated;
            }

            existing.PrimaryColor = updated.PrimaryColor;
            existing.SecondaryColor = updated.SecondaryColor;
            existing.BorderColor = updated.BorderColor;
            existing.BorderWidth = updated.BorderWidth;
            existing.TitleEn = updated.TitleEn;
            existing.TitleAr = updated.TitleAr;
            existing.HeaderTextEn = updated.HeaderTextEn;
            existing.HeaderTextAr = updated.HeaderTextAr;
            existing.SignatoryName = updated.SignatoryName;
            existing.SignatoryTitleEn = updated.SignatoryTitleEn;
            existing.SignatoryTitleAr = updated.SignatoryTitleAr;
            existing.SignatureImageUrl = updated.SignatureImageUrl;
            existing.ShowLogo = updated.ShowLogo;
            existing.LogoHeight = updated.LogoHeight;
            existing.BackgroundImageUrl = updated.BackgroundImageUrl;

            await _dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
