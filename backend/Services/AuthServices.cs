// Backend/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
      _context = context;
      _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
      var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);

      if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
          throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

      return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
      var storedToken = await _context.RefreshTokens
          .Include(rt => rt.User)
          .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

      if (storedToken is null || !storedToken.IsActive)
          throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

      // Revoga o token usado (rotação)
      storedToken.RevokedAt = DateTimeOffset.UtcNow;

      var response = await GenerateAuthResponseAsync(storedToken.User);

      storedToken.ReplacedByToken = response.RefreshToken;
      await _context.SaveChangesAsync();

      return response;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
      var accessToken = GenerateAccessToken(user);
      var refreshTokenValue = GenerateRefreshTokenValue();

      var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);
      var refreshExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);
      var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

      var refreshToken = new RefreshToken
      {
          Id = Guid.NewGuid(),
          Token = refreshTokenValue,
          UserId = user.Id,
          CreatedAt = DateTimeOffset.UtcNow,
          ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshExpirationDays)
      };

      _context.RefreshTokens.Add(refreshToken);
      await _context.SaveChangesAsync();

      return new AuthResponseDto(accessToken, refreshTokenValue, expiresAt);
    }

    private string GenerateAccessToken(User user)
    {
      var claims = new[]
      {
          new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
          new Claim(JwtRegisteredClaimNames.Email, user.Email),
          new Claim("userType", user.UserType.ToString()),
          new Claim(ClaimTypes.Role, user.UserRole.ToString()),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };

      var key = new SymmetricSecurityKey(
          System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);

      var token = new JwtSecurityToken(
          issuer: _configuration["Jwt:Issuer"],
          audience: _configuration["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
          signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshTokenValue()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}