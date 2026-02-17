using AuthService.Domain.Entities;
namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> getByUsernameAsync(string username);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task <User> CreateAsync(User user);
    Task <User> UpdateAsync(User user);
    Task <User> DeleteAsync(User user);

    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task UpdateUserRoleAsync(string userId, string roleId);
    
}