using AuthService.Domain.Entities;
namespace AuthService.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(string id);
    Task<int> CountUsersByRoleIdAsync(string roleId);
    Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(string roleId);
    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(string userId);

    }