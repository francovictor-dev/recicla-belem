using Backend.Data;
using Backend.Dtos;
using Backend.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CreateDefaultUserTests : IDisposable
{
  private readonly SqliteConnection _connection = new("DataSource=:memory:");
  private readonly AppDbContext _context;
  private readonly Mock<IS3Service> _s3 = new(MockBehavior.Strict);

  public CreateDefaultUserTests()
  {
    _connection.Open();
    _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
    _context.Database.EnsureCreated();
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Cadastro_DeveCriarLoginExternoSomenteQuandoInformado(bool withExternalLogin)
  {
    var external = withExternalLogin ? new DefaultExternalLoginDto(ExternalLoginProvider.Google, "google-id") : null;
    var result = await new UserService(_context, _s3.Object).CreateDefaultAsync(
      new CreateUserDto("test@example.com", "password123"), new CreateProfileDto("Nome"), external);
    Assert.Equal(backend.Enums.UserRole.Default, result.UserRole);
    Assert.Equal(result.Id, (await _context.Profiles.SingleAsync()).UserId);
    Assert.Equal(withExternalLogin ? 1 : 0, await _context.ExternalLogins.CountAsync());
    if (withExternalLogin)
    {
      var login = await _context.ExternalLogins.SingleAsync();
      Assert.Equal(result.Id, login.UserId);
      Assert.Equal("google-id", login.ProviderId);
    }
    _s3.VerifyNoOtherCalls();
  }

  private static IFormFile Image(byte[]? bytes = null, string type = "image/png")
  {
    var stream = new MemoryStream(bytes ?? [137, 80, 78, 71, 13, 10, 26, 10]);
    return new FormFile(stream, 0, stream.Length, "Image", "avatar.png") { Headers = new HeaderDictionary(), ContentType = type };
  }

  [Fact]
  public async Task Cadastro_DeveEnviarImagemParaCaminhoDoUsuario()
  {
    string? key = null;
    _s3.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), "image/png", default))
      .Callback<Stream, string, string, CancellationToken>((_, value, _, _) => key = value)
      .ReturnsAsync("https://example.com/profile.png");
    var result = await new UserService(_context, _s3.Object).CreateDefaultAsync(
      new CreateUserDto("test@example.com", "password123"), new CreateProfileDto("Nome"), image: Image());
    Assert.Equal($"users/{result.Id}/profile.png", key);
    _s3.VerifyAll();
  }

  [Fact]
  public async Task FalhaNoUpload_DeveReverterUsuarioPerfilELogin()
  {
    _s3.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), "image/png", default))
      .ThrowsAsync(new IOException("Falha no S3"));
    await Assert.ThrowsAsync<IOException>(() => new UserService(_context, _s3.Object).CreateDefaultAsync(
      new CreateUserDto("test@example.com", "password123"), new CreateProfileDto("Nome"),
      new DefaultExternalLoginDto(ExternalLoginProvider.Google, "google-id"), Image()));
    _context.ChangeTracker.Clear();
    Assert.Empty(await _context.Users.ToListAsync());
    Assert.Empty(await _context.Profiles.ToListAsync());
    Assert.Empty(await _context.ExternalLogins.ToListAsync());
  }

  [Theory]
  [InlineData("image/jpeg", 8)]
  [InlineData("image/png", 0)]
  [InlineData("image/png", 307201)]
  [InlineData("image/png", 9)]
  public async Task ImagemInvalida_DeveRejeitarAntesDeCriarUsuario(string type, int size)
  {
    await Assert.ThrowsAsync<ArgumentException>(() => new UserService(_context, _s3.Object).CreateDefaultAsync(
      new CreateUserDto("test@example.com", "password123"), new CreateProfileDto("Nome"), image: Image(new byte[size], type)));
    Assert.Empty(await _context.Users.ToListAsync());
    _s3.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task LoginExternoInvalido_DeveRejeitarCadastro()
  {
    await Assert.ThrowsAsync<ArgumentException>(() => new UserService(_context, _s3.Object).CreateDefaultAsync(
      new CreateUserDto("test@example.com", "password123"), new CreateProfileDto("Nome"),
      new DefaultExternalLoginDto(ExternalLoginProvider.Google, " ")));
    Assert.Empty(await _context.Users.ToListAsync());
  }

  public void Dispose()
  {
    _context.Dispose();
    _connection.Dispose();
  }
}
