using AppBL.DTOs;
using AppBL.Helper;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class TrainingService : ITrainingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<CourseDto>> GetAllCoursesAsync(PagedRequestDto request, bool? activeOnly = null)
        {
            var query = _unitOfWork.Courses.GetQueryable();

            if (activeOnly.HasValue && activeOnly.Value)
                query = query.Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(c => c.TitleEn.Contains(search) || c.TitleAr.Contains(search));
            }

            query = query.OrderByDescending(c => c.StartDate);

            var paged = await query.ToPagedResponseAsync(request);
            return new PagedResponse<CourseDto>
            {
                Items = _mapper.Map<IEnumerable<CourseDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasNext = paged.HasNext,
                HasPrevious = paged.HasPrevious
            };
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CourseCreateRequest request)
        {
            var course = _mapper.Map<Course>(request);
            course.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> UpdateCourseAsync(int id, CourseUpdateRequest request)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                throw new KeyNotFoundException("Course not found.");

            _mapper.Map(request, course);
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                return false;

            _unitOfWork.Courses.Delete(course);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<CourseEnrollmentDto> EnrollInCourseAsync(int userId, EnrollmentRequest request)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId);
            if (course == null)
                throw new KeyNotFoundException("Course not found.");

            var alreadyEnrolled = await _unitOfWork.CourseEnrollments.GetQueryable()
                .AnyAsync(ce => ce.CourseId == request.CourseId && ce.UserId == userId && ce.Status != EnrollmentStatus.Rejected);

            if (alreadyEnrolled)
                throw new InvalidOperationException("You are already enrolled (or have a pending enrollment) in this course.");

            var enrollment = new CourseEnrollment
            {
                CourseId = request.CourseId,
                UserId = userId,
                Status = EnrollmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CourseEnrollments.AddAsync(enrollment);
            await _unitOfWork.CompleteAsync();

            var savedEnrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                .Include(ce => ce.Course)
                .Include(ce => ce.User)
                .FirstAsync(ce => ce.Id == enrollment.Id);

            return _mapper.Map<CourseEnrollmentDto>(savedEnrollment);
        }

        public async Task<PagedResponse<CourseEnrollmentDto>> GetUserEnrollmentsAsync(int userId, PagedRequestDto request)
        {
            var query = _unitOfWork.CourseEnrollments.GetQueryable()
                .Include(ce => ce.Course)
                .Include(ce => ce.User)
                .Where(ce => ce.UserId == userId)
                .OrderByDescending(ce => ce.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return MapEnrollmentPage(paged);
        }

        public async Task<PagedResponse<CourseEnrollmentDto>> GetAllEnrollmentsAsync(PagedRequestDto request, EnrollmentStatus? status = null)
        {
            IQueryable<CourseEnrollment> query = _unitOfWork.CourseEnrollments.GetQueryable()
                .Include(ce => ce.Course)
                .Include(ce => ce.User);

            if (status.HasValue)
                query = query.Where(ce => ce.Status == status.Value);

            query = query.OrderByDescending(ce => ce.CreatedAt);

            var paged = await query.ToPagedResponseAsync(request);
            return MapEnrollmentPage(paged);
        }

        public async Task<CourseEnrollmentDto> UpdateEnrollmentStatusAsync(int enrollmentId, EnrollmentStatusRequest request)
        {
            var enrollment = await _unitOfWork.CourseEnrollments.GetQueryable()
                .Include(ce => ce.Course)
                .Include(ce => ce.User)
                .FirstOrDefaultAsync(ce => ce.Id == enrollmentId);

            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment record not found.");

            enrollment.Status = request.Status;
            _unitOfWork.CourseEnrollments.Update(enrollment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CourseEnrollmentDto>(enrollment);
        }

        private PagedResponse<CourseEnrollmentDto> MapEnrollmentPage(PagedResponse<CourseEnrollment> paged)
        {
            return new PagedResponse<CourseEnrollmentDto>
            {
                Items = _mapper.Map<IEnumerable<CourseEnrollmentDto>>(paged.Items),
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
