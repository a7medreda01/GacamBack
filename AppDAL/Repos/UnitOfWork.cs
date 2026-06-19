using AppDAL.Context;
using AppDAL.Entities;
using AppDAL.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppDAL.Repos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            Roles = new Repository<Role>(_context);
            UserRoles = new Repository<UserRole>(_context);
            Pages = new Repository<Page>(_context);
            News = new Repository<News>(_context);
            Partners = new Repository<Partner>(_context);
            ServiceFees = new Repository<ServiceFee>(_context);
            AccreditationCategories = new Repository<AccreditationCategory>(_context);
            MediaAccreditations = new Repository<MediaAccreditation>(_context);
            MediaCards = new Repository<MediaCard>(_context);
            Courses = new Repository<Course>(_context);
            CourseEnrollments = new Repository<CourseEnrollment>(_context);
            Volunteers = new Repository<Volunteer>(_context);
            Certificates = new Repository<Certificate>(_context);
            Payments = new Repository<Payment>(_context);
            Orders = new Repository<Order>(_context);
            OrderStatusHistories = new Repository<OrderStatusHistory>(_context);
            AuditLogs = new Repository<AuditLog>(_context);
            Settings = new Repository<Setting>(_context);
            CertificateDesigns = new Repository<CertificateDesign>(_context);
        }

        public IRepository<User> Users { get; }
        public IRepository<Role> Roles { get; }
        public IRepository<UserRole> UserRoles { get; }
        public IRepository<Page> Pages { get; }
        public IRepository<News> News { get; }
        public IRepository<Partner> Partners { get; }
        public IRepository<ServiceFee> ServiceFees { get; }
        public IRepository<AccreditationCategory> AccreditationCategories { get; }
        public IRepository<MediaAccreditation> MediaAccreditations { get; }
        public IRepository<MediaCard> MediaCards { get; }
        public IRepository<Course> Courses { get; }
        public IRepository<CourseEnrollment> CourseEnrollments { get; }
        public IRepository<Volunteer> Volunteers { get; }
        public IRepository<Certificate> Certificates { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<Order> Orders { get; }
        public IRepository<OrderStatusHistory> OrderStatusHistories { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<Setting> Settings { get; }
        public IRepository<CertificateDesign> CertificateDesigns { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
