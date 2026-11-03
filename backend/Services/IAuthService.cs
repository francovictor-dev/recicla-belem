// Backend/Services/IAuthService.cs
using Backend.Dtos;

namespace Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
}