using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class UserManagementService(
    IUserRepository users, 
    IRoleRepository roles, 
    ICloudinaryService cloudinary) : IUserManagementService
{
    public async Task<UserResponseDto> GetUserProfileAsync(string userId)
    {
        var user = await users.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("User not found");
        
        return MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid userId", nameof(userId));
        
        if (!RoleConstants.AllowedRoles.Contains(roleName))
            throw new InvalidOperationException($"Role not allowed. Use {RoleConstants.ADMIN_ROLE} or {RoleConstants.USER_ROLE}");

        var user = await users.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");

        // Evitar dejar el sistema sin admins
        var isUserAdmin = user.UserRoles.Any(r => r.Role.Name == RoleConstants.ADMIN_ROLE);
        if (isUserAdmin && roleName != RoleConstants.ADMIN_ROLE)
        {
            var adminCount = await roles.CountUsersInRoleAsync(RoleConstants.ADMIN_ROLE);
            if (adminCount <= 1)
                throw new InvalidOperationException("Cannot remove the last administrator");
        }

        var role = await roles.GetByNameAsync(roleName)
                   ?? throw new InvalidOperationException($"Role {roleName} not found");

        await users.UpdateUserRoleAsync(userId, role.Id);

        // Recargar para devolver los datos actualizados
        var updatedUser = await users.GetByIdAsync(userId);
        return MapToUserResponseDto(updatedUser!);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
    {
        return await roles.GetUserRoleNamesAsync(userId);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;
        var usersInRole = await roles.GetUsersByRoleAsync(roleName);
        
        return usersInRole.Select(MapToUserResponseDto).ToList();
    }

    // Método privado para evitar repetir la lógica de Cloudinary y mapeo
  private UserResponseDto MapToUserResponseDto(User u)
{
    return new UserResponseDto(
        Guid.TryParse(u.Id, out var guidId) ? guidId : Guid.Empty, // Conversión segura
        u.Email,
        u.Name,
        u.Surname,
        u.Username,
        cloudinary.GetFullImageUrl(u.UserProfile?.ProfilePicture ?? string.Empty),
        u.UserProfile?.Phone ?? string.Empty,
        u.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE,
        u.Status,
        u.UserEmail?.EmailVerified ?? false,
        u.CreatedAt,
        u.UpdatedAt
    );
}
}