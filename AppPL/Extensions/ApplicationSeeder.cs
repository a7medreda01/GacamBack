using AppDAL.Context;
using AppDAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppPL.Extensions
{
    /// <summary>
    /// Application-level seeder that runs in AppPL where BCrypt is available via AppBL.
    /// Complements the DatabaseSeeder in AppDAL which handles pages and service fees only.
    /// </summary>
    public static class ApplicationSeeder
    {
        public static async Task SeedRolesAndAdminAsync(AppDbContext context)
        {
            // 1. Seed Roles (no explicit IDs)
            if (!await context.Roles.AnyAsync())
            {
                await context.Roles.AddRangeAsync(
                    new Role { Name = "Admin" },
                    new Role { Name = "Employee" },
                    new Role { Name = "User" },
                    new Role { Name = "Volunteer" }
                );
                await context.SaveChangesAsync();
            }

            // 2. Seed default Admin user (password is hashed here where BCrypt is available)
            if (!await context.Users.AnyAsync(u => u.Email == "admin@gacam.media"))
            {
                var admin = new User
                {
                    Email       = "admin@gacam.media",
                    FullName    = "GACAM Admin",
                    PhoneNumber = "+1234567890",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@Gacam2026"),
                    IsActive    = true,
                    CreatedAt   = DateTime.UtcNow
                };

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();

                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
