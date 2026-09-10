// Backend/Dtos/AuthDtos.cs
namespace Backend.Dtos;

public record LoginDto(string Email, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
public record RefreshTokenRequestDto(string RefreshToken);
