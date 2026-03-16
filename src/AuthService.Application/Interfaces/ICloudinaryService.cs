using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string fileName); // Cambiado a IFormFile
    string GetDefaultAvatarUrl();
    string GetFullImageUrl(string publicId);
}