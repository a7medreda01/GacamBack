using AutoMapper;
using AppDAL.Entities;
using AppBL.DTOs;

namespace AppBL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));

            CreateMap<Page, PageDto>()
                .ForMember(dest => dest.UpdatedByUserName, opt => opt.MapFrom(src => src.UpdatedByUser != null ? src.UpdatedByUser.FullName : null));
            CreateMap<PageUpdateRequest, Page>();

            CreateMap<News, NewsDto>();
            CreateMap<NewsCreateRequest, News>();
            CreateMap<NewsUpdateRequest, News>();

            CreateMap<Partner, PartnerDto>();
            CreateMap<PartnerCreateRequest, Partner>();
            CreateMap<PartnerUpdateRequest, Partner>();

            CreateMap<Volunteer, VolunteerDto>();
            CreateMap<VolunteerRegisterRequest, Volunteer>();

            CreateMap<Course, CourseDto>();
            CreateMap<CourseCreateRequest, Course>();
            CreateMap<CourseUpdateRequest, Course>();

            CreateMap<CourseEnrollment, CourseEnrollmentDto>()
                .ForMember(dest => dest.CourseTitleEn, opt => opt.MapFrom(src => src.Course.TitleEn))
                .ForMember(dest => dest.CourseTitleAr, opt => opt.MapFrom(src => src.Course.TitleAr))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<ServiceFee, ServiceFeeDto>();

            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.VerifiedByUserName, opt => opt.MapFrom(src => src.VerifiedByUser != null ? src.VerifiedByUser.FullName : null));
            CreateMap<PaymentSubmitRequest, Payment>();

            CreateMap<Certificate, CertificateDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<MediaAccreditation, AccreditationDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.AccreditationCategoryId))
                .ForMember(dest => dest.CategoryNameEn, opt => opt.MapFrom(src => src.AccreditationCategory.NameEn))
                .ForMember(dest => dest.CategoryNameAr, opt => opt.MapFrom(src => src.AccreditationCategory.NameAr))
                .ForMember(dest => dest.CheckedByUserName, opt => opt.MapFrom(src => src.CheckedByUser != null ? src.CheckedByUser.FullName : null));

            CreateMap<MediaCard, MediaCardDto>();

            CreateMap<AccreditationCategory, AccreditationCategoryDto>();
            CreateMap<CreateAccreditationCategoryDto, AccreditationCategory>();
            CreateMap<UpdateAccreditationCategoryDto, AccreditationCategory>();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
                .ForMember(dest => dest.ChangedByUserName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : null));

            CreateMap<AuditLog, AuditLogDto>();
        }
    }
}
