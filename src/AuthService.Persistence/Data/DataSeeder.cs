using AuthService.Domain; 
using AuthService.Domain.Entities; 
using AuthService.Application.Services;
using AuthService.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
       // 1. Verificar roles
        if (!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new() { Id = UuidGenerator.GenerateRoleId(), Name = RoleConstants.ADMIN_ROLE },
                new() { Id = UuidGenerator.GenerateRoleId(), Name = RoleConstants.USER_ROLE }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed de admin
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.ADMIN_ROLE);
            if (adminRole != null)
            {
                // Usamos el generador de string para el ID
                string userId = UuidGenerator.GenerateUserId(); 

                var adminUser = new User
                {
                    Id = userId, 
                    Username = "admin",
                    Email = "admin@ksports.local",
                    PasswordHash = "12345678", 
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    UserProfile = new UserProfile
                    {
                        Id = UuidGenerator.GenerateUserId(),
                        UserId = userId // Ahora ambos son string
                    },
                    UserEmail = new UserEmail
                    {
                        Id = UuidGenerator.GenerateUserId(),
                        UserId = userId,
                        EmailVerified = true
                    },
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Id = UuidGenerator.GenerateUserId(),
                            UserId = userId,
                            RoleId = adminRole.Id
                        }
                    }
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}