using AuthService.Domain.Entities;
namespace AuthService.Domain;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Client"; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propiedades de navegación para las relaciones que definimos
    public virtual UserProfile? UserProfile { get; set; }
    public virtual UserEmail? UserEmail { get; set; }
    public virtual UserPasswordReset? UserPasswordReset { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}