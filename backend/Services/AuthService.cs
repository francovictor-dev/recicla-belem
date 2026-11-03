using Backend.Enums;
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
    private readonly IUserService _users;
    private readonly IGoogleTokenValidator _google;

    public AuthService(AppDbContext context, IConfiguration configuration, IUserService users, IGoogleTokenValidator google)
    {
      _context = context;
      _configuration = configuration;
      _users = users;
      _google = google;
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
      var identity = await _google.ValidateAsync(dto.IdToken);
      var linked = await _context.ExternalLogins.Include(login => login.User)
          .SingleOrDefaultAsync(login => login.Provider == ExternalLoginProvider.Google && login.ProviderId == identity.Subject);
      if (linked is not null)
      {
        if (!linked.User.IsActived) throw new UnauthorizedAccessException("Usuário inativo.");
        return await GenerateAuthResponseAsync(linked.User);
      }

      var email = identity.Email.Trim().ToLowerInvariant();
      var user = await _context.Users.Include(user => user.Profile).SingleOrDefaultAsync(user => user.Email.ToLower() == email);
      if (user is null)
      {
        var created = await _users.CreateDefaultAsync(
            new CreateUserDto(email, Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)), UserType.Google),
            new CreateProfileDto(identity.Name ?? email, GetPhotoUrl(identity.Picture)),
            new DefaultExternalLoginDto(ExternalLoginProvider.Google, identity.Subject));
        user = await _context.Users.SingleAsync(user => user.Id == created.Id);
      }
      else
      {
        if (!user.IsActived) throw new UnauthorizedAccessException("Usuário inativo.");
        if (await _context.ExternalLogins.AnyAsync(login => login.UserId == user.Id && login.Provider == ExternalLoginProvider.Google))
          throw new UnauthorizedAccessException("O usuário já está vinculado a outra conta Google.");
        // Google is authoritative for Gmail and verified Workspace addresses.
        if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(identity.HostedDomain))
          throw new UnauthorizedAccessException("Entre com sua senha para confirmar o vínculo deste e-mail à conta Google.");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var now = DateTimeOffset.UtcNow;
        _context.ExternalLogins.Add(new ExternalLogin
        {
          Id = Guid.NewGuid(), Provider = ExternalLoginProvider.Google, ProviderId = identity.Subject,
          UserId = user.Id, CreatedAt = now, UpdatedAt = now
        });
        var photo = GetPhotoUrl(identity.Picture);
        if (user.Profile is null)
          _context.Profiles.Add(new Profile { Id = Guid.NewGuid(), UserId = user.Id, Name = identity.Name ?? email, PhotoUrl = photo });
        else if (photo is not null && string.IsNullOrWhiteSpace(user.Profile.PhotoUrl)) user.Profile.PhotoUrl = photo;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
      }
      return await GenerateAuthResponseAsync(user);
    }

    private static string? GetPhotoUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? value : null;

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