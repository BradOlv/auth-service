namespace AuthService.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty; 
    public string Name { get; set; } = string.Empty; // Añadido
    public string Surname { get; set; } = string.Empty; // Añadido
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Client"; 
    public bool Status { get; set; } = false; // Añadido (false hasta que verifique email)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } // Añadido

    // Propiedades de navegación
    public virtual UserProfile? UserProfile { get; set; }
    public virtual UserEmail? UserEmail { get; set; }
    public virtual UserPasswordReset? UserPasswordReset { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}