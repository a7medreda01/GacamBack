using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IServiceFeeService
    {
        Task<IEnumerable<ServiceFeeDto>> GetAllFeesAsync();
        Task<ServiceFeeDto> UpdateFeeAsync(
    OrderType orderType,
    ServiceFeeUpdateRequest request);
    }
}
