using AuthService.Application.DTOs; 
using AuthService.Application.DTOs.Email;

namespace AuthService.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserResponseDto> GetUserProfileAsync(string userId);
    Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName);
    Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName);
}