using Backend.Data;
using Backend.Dtos;
using Backend.Enums;
using Backend.Models;
using Backend.Services;
using Google.Apis.Auth;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Backend.Tests;

public class GoogleLoginTests : IDisposable
{
  private readonly SqliteConnection _connection = new("DataSource=:memory:");
  private readonly AppDbContext _context;
  private readonly Mock<IGoogleTokenValidator> _validator = new();
  private readonly GoogleJsonWebSignature.Payload _identity = new()
  {
    Subject = "google-openid", Email = "person@gmail.com", EmailVerified = true, Name = "Maria", Picture = "https://example.com/avatar.jpg"
  };

  public GoogleLoginTests()
  {
    _connection.Open();
    _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
    _context.Database.EnsureCreated();
    _validator.Setup(v => v.ValidateAsync("id-token")).ReturnsAsync(_identity);
  }

  private AuthService Service()
  {
    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["Jwt:Key"] = "test-key-at-least-32-characters-long-for-hmac-sha256",
      ["Jwt:Issuer"] = "test", ["Jwt:Audience"] = "test", ["Jwt:AccessTokenExpirationMinutes"] = "15", ["Jwt:RefreshTokenExpirationDays"] = "7"
    }).Build();
    return new AuthService(_context, config, new UserService(_context, Mock.Of<IS3Service>()), _validator.Object);
  }

  [Fact]
  public async Task GoogleLogin_DeveCadastrarPerfilLoginExternoEEmitirTokensSemCriarImage()
  {
    var result = await Service().GoogleLoginAsync(new("id-token"));
    Assert.NotEmpty(result.AccessToken); Assert.NotEmpty(result.RefreshToken);
    var user = await _context.Users.SingleAsync();
    Assert.Equal(UserType.Google, user.UserType);
    Assert.Equal(backend.Enums.UserRole.Default, user.UserRole);
    var profile = await _context.Profiles.SingleAsync();
    Assert.Equal("Maria", profile.Name); Assert.Equal(_identity.Picture, profile.PhotoUrl);
    var external = await _context.ExternalLogins.SingleAsync();
    Assert.Equal(user.Id, external.UserId); Assert.Equal("google-openid", external.ProviderId);
    Assert.Equal(ExternalLoginProvider.Google, external.Provider);
    Assert.Empty(await _context.Images.ToListAsync());
    await Service().GoogleLoginAsync(new("id-token"));
    Assert.Equal(1, await _context.Users.CountAsync());
    Assert.Equal(1, await _context.ExternalLogins.CountAsync());
    Assert.Equal(2, await _context.RefreshTokens.CountAsync());
  }

  [Fact]
  public async Task GoogleLogin_DeveEntrarEmContaExistenteSemVerificarSenha()
  {
    _context.Users.Add(new User { Id = Guid.NewGuid(), Email = "PERSON@gmail.com", Password = "not-a-password-hash", IsActived = true });
    await _context.SaveChangesAsync();
    var response = await Service().GoogleLoginAsync(new("id-token"));
    Assert.NotEmpty(response.AccessToken);
    Assert.Equal(1, await _context.Users.CountAsync());
    Assert.Equal("not-a-password-hash", (await _context.Users.SingleAsync()).Password);
    Assert.Equal(_identity.Picture, (await _context.Profiles.SingleAsync()).PhotoUrl);
  }

  [Fact]
  public async Task GoogleLogin_DeveRejeitarTokenInvalidoSemCriarRegistros()
  {
    _validator.Setup(v => v.ValidateAsync("id-token")).ThrowsAsync(new UnauthorizedAccessException());
    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().GoogleLoginAsync(new("id-token")));
    Assert.Empty(await _context.Users.ToListAsync());
    Assert.Empty(await _context.RefreshTokens.ToListAsync());
  }

  [Fact]
  public async Task GoogleLogin_DeveRejeitarUsuarioInativo()
  {
    _context.Users.Add(new User { Id = Guid.NewGuid(), Email = _identity.Email, IsActived = false });
    await _context.SaveChangesAsync();
    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().GoogleLoginAsync(new("id-token")));
    Assert.Empty(await _context.ExternalLogins.ToListAsync());
  }

  [Fact]
  public async Task GoogleLogin_NaoDeveVincularOutroGoogleAoMesmoUsuario()
  {
    var user = new User { Id = Guid.NewGuid(), Email = _identity.Email, IsActived = true };
    _context.Users.Add(user);
    _context.ExternalLogins.Add(new ExternalLogin { Id = Guid.NewGuid(), UserId = user.Id, Provider = ExternalLoginProvider.Google, ProviderId = "other-sub" });
    await _context.SaveChangesAsync();
    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().GoogleLoginAsync(new("id-token")));
    Assert.Equal(1, await _context.ExternalLogins.CountAsync());
  }

  [Fact]
  public async Task GoogleLogin_NaoDeveVincularEmailDeTerceiroSemConfirmacao()
  {
    _identity.Email = "person@example.com";
    _context.Users.Add(new User { Id = Guid.NewGuid(), Email = _identity.Email, IsActived = true });
    await _context.SaveChangesAsync();
    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().GoogleLoginAsync(new("id-token")));
    Assert.Empty(await _context.ExternalLogins.ToListAsync());
  }

  public void Dispose() { _context.Dispose(); _connection.Dispose(); }
}
