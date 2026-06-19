using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VolunteerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VolunteerDto> RegisterVolunteerAsync(int userId, VolunteerRegisterRequest request)
        {
            var alreadyApplied = await _unitOfWork.Volunteers.GetQueryable()
                .AnyAsync(v => v.UserId == userId && v.Status == ApplicationStatus.Pending);

            if (alreadyApplied)
                throw new InvalidOperationException("You already have a pending volunteer application.");

            var volunteer = _mapper.Map<Volunteer>(request);
            volunteer.UserId = userId;
            volunteer.Status = ApplicationStatus.Pending;
            volunteer.AppliedAt = DateTime.UtcNow;

            await _unitOfWork.Volunteers.AddAsync(volunteer);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public async Task<IEnumerable<VolunteerDto>> GetAllVolunteersAsync(ApplicationStatus? status = null)
        {
            var query = _unitOfWork.Volunteers.GetQueryable().Include(v => v.User);
            
            if (status.HasValue)
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Volunteer, User>)query.Where(v => v.Status == status.Value);
            }

            var volunteers = await query.OrderByDescending(v => v.AppliedAt).ToListAsync();
            return _mapper.Map<IEnumerable<VolunteerDto>>(volunteers);
        }

        public async Task<VolunteerDto?> GetVolunteerByIdAsync(int id)
        {
            var volunteer = await _unitOfWork.Volunteers.GetQueryable()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (volunteer == null)
                return null;

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public async Task<VolunteerDto?> GetVolunteerByUserIdAsync(int userId)
        {
            var volunteer = await _unitOfWork.Volunteers.GetQueryable()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null)
                return null;

            return _mapper.Map<VolunteerDto>(volunteer);
        }

        public async Task<VolunteerDto> UpdateVolunteerStatusAsync(int id, VolunteerStatusUpdateRequest request)
        {
            var volunteer = await _unitOfWork.Volunteers.GetQueryable()
                .Include(v => v.User)
                    .ThenInclude(u => u.UserRoles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (volunteer == null)
                throw new KeyNotFoundException("Volunteer record not found.");

            volunteer.Status = request.Status;
            _unitOfWork.Volunteers.Update(volunteer);

            // لو الطلب اتقبل → نضيف role رقم 4 (Volunteer) للـ user لو مش موجودة عنده
            if (request.Status == ApplicationStatus.Approved)
            {
                const int volunteerRoleId = 4;

                bool alreadyHasRole = volunteer.User.UserRoles
                    .Any(ur => ur.RoleId == volunteerRoleId);

                if (!alreadyHasRole)
                {
                    var userRole = new UserRole
                    {
                        UserId = volunteer.UserId,
                        RoleId = volunteerRoleId
                    };
                    await _unitOfWork.UserRoles.AddAsync(userRole);
                }
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<VolunteerDto>(volunteer);
        }
    }
}
