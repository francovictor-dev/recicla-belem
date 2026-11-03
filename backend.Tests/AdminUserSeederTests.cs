using Backend.Data;
using Backend.Data.Seeders;
using Backend.Enums;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Backend.Tests;

public class AdminUserSeederTests
{
  private static AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
  private static IConfiguration Config(string? email = "admin@example.com", string? password = "Test&Pw123!") =>
      new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["USER_ADMIN_EMAIL"] = email, ["USER_ADMIN_PW"] = password }).Build();

  [Fact]
  public async Task SeedAsync_DeveCriarAdminEmailComSenhaHasheada()
  {
    await using var context = Context();
    await AdminUserSeeder.SeedAsync(context, Config());
    var user = await context.Users.SingleAsync();
    Assert.Equal("admin@example.com", user.Email);
    Assert.Equal(backend.Enums.UserRole.Admin, user.UserRole); Assert.Equal(UserType.Email, user.UserType);
    Assert.True(user.IsActived); Assert.NotEqual(Guid.Empty, user.Id);
    Assert.NotEqual("Test&Pw123!", user.Password); Assert.True(BCrypt.Net.BCrypt.Verify("Test&Pw123!", user.Password));
    Assert.NotEqual(default, user.CreatedAt); Assert.Equal(user.CreatedAt, user.UpdatedAt);
  }

  [Fact]
  public async Task SeedAsync_NaoDeveDuplicarOuTrocarSenhaDoAdminExistente()
  {
    await using var context = Context();
    await AdminUserSeeder.SeedAsync(context, Config());
    var original = await context.Users.SingleAsync(); var hash = original.Password;
    await AdminUserSeeder.SeedAsync(context, Config("ADMIN@example.com"));
    var result = await context.Users.SingleAsync();
    Assert.Equal(original.Id, result.Id); Assert.Equal(hash, result.Password);
  }

  [Fact]
  public async Task SeedAsync_DeveGarantirAdminEmailESenhaNaContaExistente()
  {
    await using var context = Context();
    context.Users.Add(new User { Id = Guid.NewGuid(), Email = "admin@example.com", Password = "original", UserRole = backend.Enums.UserRole.Default, UserType = UserType.Google });
    await context.SaveChangesAsync();
    await AdminUserSeeder.SeedAsync(context, Config());
    var result = await context.Users.SingleAsync();
    Assert.Equal(backend.Enums.UserRole.Admin, result.UserRole);
    Assert.Equal(UserType.Email, result.UserType);
    Assert.True(BCrypt.Net.BCrypt.Verify("Test&Pw123!", result.Password));
  }

  [Fact]
  public async Task SeedAsync_DeveAplicarNovaSenhaConfiguradaSemDuplicarConta()
  {
    await using var context = Context();
    await AdminUserSeeder.SeedAsync(context, Config());
    var user = await context.Users.SingleAsync(); var id = user.Id;
    await AdminUserSeeder.SeedAsync(context, Config(password: "NewPassword123!"));
    Assert.Equal(id, (await context.Users.SingleAsync()).Id);
    Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword123!", user.Password));
  }

  [Fact]
  public async Task SeedAsync_NaoDeveCriarSemConfiguracao()
  {
    await using var context = Context();
    await AdminUserSeeder.SeedAsync(context, Config(null, null));
    Assert.Empty(await context.Users.ToListAsync());
  }

  [Theory]
  [InlineData("admin@example.com", null)]
  [InlineData(null, "password")]
  [InlineData("invalido", "password")]
  public async Task SeedAsync_DeveRejeitarConfiguracaoParcialOuInvalida(string? email, string? password)
  {
    await using var context = Context();
    await Assert.ThrowsAsync<InvalidOperationException>(() => AdminUserSeeder.SeedAsync(context, Config(email, password)));
    Assert.Empty(await context.Users.ToListAsync());
  }
}
