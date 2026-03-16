namespace AuthService.Domain.Entities;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; } // Añadido
    public string? Phone { get; set; } // Añadido

    public virtual User User { get; set; } = null!;
}