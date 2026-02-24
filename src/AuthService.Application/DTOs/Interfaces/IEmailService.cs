namespace AuthService.Application.Interfaces;
 
public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string username, string verificationToken);
    Task SendPasswordResetAsync(string email, string username, string Token);
    Task SendWelcomeEmailAsync(string email, string username);
}