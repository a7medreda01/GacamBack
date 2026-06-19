using AppBL.IService;
using AppDAL.Context;
using AppDAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppBL.Service
{
    /// <summary>
    /// Service for CRUD operations on the singleton Setting entity.
    /// </summary>
    public class SettingService : ISettingService
    {
        private readonly AppDbContext _dbContext;

        public SettingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Setting> GetAsync()
        {
            var setting = await _dbContext.Settings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new Setting
                {
                    SiteTitleEn = "GACAM",
                    SiteTitleAr = "الهيئة العامة للإعلام",
                    LogoUrl = string.Empty,
                    SocialLinksJson = "{}",
                    ContactInfo = "{}"
                };
                _dbContext.Settings.Add(setting);
                await _dbContext.SaveChangesAsync();
            }
            return setting;
        }

        public async Task<Setting> UpdateAsync(Setting updated)
        {
            var existing = await _dbContext.Settings.FirstOrDefaultAsync();
            if (existing == null)
            {
                _dbContext.Settings.Add(updated);
                await _dbContext.SaveChangesAsync();
                return updated;
            }

            existing.SiteTitleEn = updated.SiteTitleEn;
            existing.SiteTitleAr = updated.SiteTitleAr;
            existing.LogoUrl = updated.LogoUrl;
            existing.SocialLinksJson = updated.SocialLinksJson;
            existing.ContactInfo = updated.ContactInfo;

            await _dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
