using AppDAL.Entities;

namespace AppDAL.IRepos
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Page> Pages { get; }
        IRepository<News> News { get; }
        IRepository<Partner> Partners { get; }
        IRepository<ServiceFee> ServiceFees { get; }
        IRepository<AccreditationCategory> AccreditationCategories { get; }
        IRepository<MediaAccreditation> MediaAccreditations { get; }
        IRepository<MediaCard> MediaCards { get; }
        IRepository<Course> Courses { get; }
        IRepository<CourseEnrollment> CourseEnrollments { get; }
        IRepository<Volunteer> Volunteers { get; }
        IRepository<Certificate> Certificates { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderStatusHistory> OrderStatusHistories { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<Setting> Settings { get; }
        IRepository<CertificateDesign> CertificateDesigns { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
