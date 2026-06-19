using System.Threading.Tasks;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface ICertificateDesignService
    {
        Task<CertificateDesign> GetAsync();
        Task<CertificateDesign> UpdateAsync(CertificateDesign updated);
    }
}
