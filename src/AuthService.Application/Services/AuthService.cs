using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Exceptions;
using AuthService.Application.Extensions;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; 

namespace AuthService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    ICloudinaryService cloudinaryService,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await userRepository.ExistsByEmailAsync(registerDto.Email))
            throw new BusinessException("EMAIL_ALREADY_EXISTS", "Email already exists");

        if (await userRepository.ExistsByUsernameAsync(registerDto.Username))
            throw new BusinessException("USERNAME_ALREADY_EXISTS", "Username already exists");

        string profilePicturePath = registerDto.ProfilePicture != null && registerDto.ProfilePicture.Length > 0
            ? await _cloudinaryService.UploadImageAsync(registerDto.ProfilePicture, registerDto.ProfilePicture.FileName)
            : _cloudinaryService.GetDefaultAvatarUrl();

        var userId = Guid.NewGuid().ToString(); 
        var emailToken = Guid.NewGuid().ToString(); 

        var user = new User
        {
            Id = userId,
            Name = registerDto.Name,
            Surname = registerDto.Surname,
            Username = registerDto.Username,
            Email = registerDto.Email.ToLowerInvariant(),
            PasswordHash = passwordHashService.HashPassword(registerDto.Password),
            Status = false,
            CreatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                ProfilePicture = profilePicturePath,
                Phone = registerDto.Phone
            },
            UserEmail = new UserEmail
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                EmailVerified = false,
                EmailVerificationToken = emailToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            }
        };

        var createdUser = await userRepository.CreateAsync(user);

        _ = emailService.SendEmailVerificationAsync(createdUser.Email, createdUser.Username, emailToken);

        return new RegisterResponseDto
        {
            Success = true,
            User = MapToUserResponseDto(createdUser),
            Message = "Registro exitoso. Verifica tu email.",
            EmailVerificationRequired = true
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = loginDto.EmailOrUsername.Contains('@')
            ? await userRepository.GetByEmailAsync(loginDto.EmailOrUsername.ToLowerInvariant())
            : await userRepository.GetByUsernameAsync(loginDto.EmailOrUsername);

        if (user == null || !passwordHashService.VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        if (!user.Status)
            throw new UnauthorizedAccessException("Cuenta deshabilitada. Verifica tu email.");

        var token = jwtTokenService.GenerateToken(user);
        
        return new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso",
            Token = token,
            UserDetails = MapToUserDetailsDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto(
            Guid.Parse(user.Id),
            user.Email,
            user.Name,
            user.Surname,
            user.Username,
            user.UserProfile?.ProfilePicture ?? "",
            user.UserProfile?.Phone ?? "",
            user.Role,
            user.Status,
            user.UserEmail?.EmailVerified ?? false,
            user.CreatedAt,
            user.UpdatedAt
        );
    }

    private UserDetailsDto MapToUserDetailsDto(User user)
    {
        return new UserDetailsDto(
            Guid.Parse(user.Id),
            user.Username,
            user.UserProfile?.ProfilePicture ?? "",
            user.Role
        );
    }

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto { Success = false, Message = "Invalid or expired verification token" };
        }

        user.UserEmail.EmailVerified = true;
        user.Status = true;
        user.UserEmail.EmailVerificationToken = null;
        user.UserEmail.EmailVerificationTokenExpiry = null;

        await userRepository.UpdateAsync(user);

        try { await emailService.SendWelcomeEmailAsync(user.Email, user.Username); }
        catch (Exception ex) { logger.LogError(ex, "Failed to send welcome email"); }

        return new EmailResponseDto { Success = true, Message = "Email verificado exitosamente" };
    }

    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        if (user == null || user.UserEmail == null)
            return new EmailResponseDto { Success = false, Message = "Usuario no encontrado" };

        if (user.UserEmail.EmailVerified)
            return new EmailResponseDto { Success = false, Message = "El email ya ha sido verificado" };

        var newToken = Guid.NewGuid().ToString();
        user.UserEmail.EmailVerificationToken = newToken;
        user.UserEmail.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await userRepository.UpdateAsync(user);

        await emailService.SendEmailVerificationAsync(user.Email, user.Username, newToken);
        return new EmailResponseDto { Success = true, Message = "Email de verificación enviado" };
    }

    public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null) return new EmailResponseDto { Success = true, Message = "Si el email existe, se envió un enlace" };

        var resetToken = Guid.NewGuid().ToString();
        if (user.UserPasswordReset == null)
        {
            user.UserPasswordReset = new UserPasswordReset { UserId = user.Id, PasswordResetToken = resetToken, PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1) };
        }
        else
        {
            user.UserPasswordReset.PasswordResetToken = resetToken;
            user.UserPasswordReset.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        }

        await userRepository.UpdateAsync(user);
        await emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken);

        return new EmailResponseDto { Success = true, Message = "Si el email existe, se envió un enlace" };
    }

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordDto.Token);
        if (user == null || user.UserPasswordReset == null)
            return new EmailResponseDto { Success = false, Message = "Token inválido" };

        user.PasswordHash = passwordHashService.HashPassword(resetPasswordDto.NewPassword);
        user.UserPasswordReset.PasswordResetToken = null;
        user.UserPasswordReset.PasswordResetTokenExpiry = null;

        await userRepository.UpdateAsync(user);
        return new EmailResponseDto { Success = true, Message = "Contraseña actualizada" };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserResponseDto(user);
    }
}