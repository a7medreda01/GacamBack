using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface IVolunteerService
    {
        Task<VolunteerDto> RegisterVolunteerAsync(int userId, VolunteerRegisterRequest request);
        Task<IEnumerable<VolunteerDto>> GetAllVolunteersAsync(ApplicationStatus? status = null);
        Task<VolunteerDto?> GetVolunteerByIdAsync(int id);
        Task<VolunteerDto?> GetVolunteerByUserIdAsync(int userId);
        Task<VolunteerDto> UpdateVolunteerStatusAsync(int id, VolunteerStatusUpdateRequest request);
    }
}
