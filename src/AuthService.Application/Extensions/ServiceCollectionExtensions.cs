using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Interfaces;
using AuthService.Persistence.Data;
using AuthService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Configuration;       

namespace AuthService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DB CONTEXT
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                        .UseSnakeCaseNamingConvention());

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Servicios de Aplicación - Usando rutas completas para forzar la detección
        services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();
        services.AddScoped<IUserManagementService, AuthService.Application.Services.UserManagementService>();
        
        // CORRECCIÓN AQUÍ: Si fallan, revisa que los archivos existan en /Services
       services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<ICloudinaryService, AuthService.Application.Services.CloudinaryService>();
        services.AddScoped<IEmailService, AuthService.Application.Services.EmailService>();

        services.AddHealthChecks();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
}