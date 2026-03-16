using Microsoft.AspNetCore.Http;

namespace AuthService.Application.DTOs;

public record RegisterDto(string Name, string Surname, string Username, string Email, string Password, string Phone, IFormFile? ProfilePicture);

public record LoginDto(string EmailOrUsername, string Password);

public record AuthResponseDto 
{ 
    public bool Success { get; init; } 
    public string Message { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public UserDetailsDto? UserDetails { get; init; } 
    public DateTime ExpiresAt { get; init; } 
}

public record RegisterResponseDto 
{ 
    public bool Success { get; init; } 
    public UserResponseDto? User { get; init; } 
    public string Message { get; init; } = string.Empty;
    public bool EmailVerificationRequired { get; init; } 
}

public record EmailResponseDto 
{ 
    public bool Success { get; init; } 
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; } // Esto arregla los errores de .Data
}

public record VerifyEmailDto(string Token);
public record ResendVerificationDto(string Email);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Token, string NewPassword);

public record UserDetailsDto(Guid Id, string Username, string ProfilePicture, string Role);

public record UserResponseDto(
    Guid Id, 
    string Email, 
    string Name, 
    string Surname, 
    string Username, 
    string ProfilePicture, 
    string Phone, 
    string Role, 
    bool Status, 
    bool IsEmailVerified, 
    DateTime CreatedAt, 
    DateTime? UpdatedAt
);