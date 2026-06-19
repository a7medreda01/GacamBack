using System.Threading.Tasks;
using AppDAL.Entities;

namespace AppBL.IService
{
    /// <summary>
    /// Service for managing site-wide settings (title, logo, social links, contact info).
    /// </summary>
    public interface ISettingService
    {
        Task<Setting> GetAsync();
        Task<Setting> UpdateAsync(Setting updated);
    }
}
