using AppBL.DTOs;
using AppDAL.Entities;

namespace AppBL.IService
{
    public interface ITrainingService
    {
        Task<PagedResponse<CourseDto>> GetAllCoursesAsync(PagedRequestDto request, bool? activeOnly = null);
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CourseCreateRequest request);
        Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateRequest request);
        Task<bool> DeleteCourseAsync(int id);
        Task<CourseEnrollmentDto> EnrollInCourseAsync(int userId, EnrollmentRequest request);
        Task<PagedResponse<CourseEnrollmentDto>> GetUserEnrollmentsAsync(int userId, PagedRequestDto request);
        Task<PagedResponse<CourseEnrollmentDto>> GetAllEnrollmentsAsync(PagedRequestDto request, EnrollmentStatus? status = null);
        Task<CourseEnrollmentDto> UpdateEnrollmentStatusAsync(int enrollmentId, EnrollmentStatusRequest request);
    }
}
