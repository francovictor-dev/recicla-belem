using Backend.Data;
using Backend.Dtos;
using Backend.Enums;
using Backend.Models;
using Backend.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Backend.Tests;

public class TransactionExecutionStrategyTests
{
  private sealed class RetryFactory(ExecutionStrategyDependencies dependencies) : IExecutionStrategyFactory
  {
    public IExecutionStrategy Create() => new RetryStrategy(dependencies);
  }

  private sealed class RetryStrategy(ExecutionStrategyDependencies dependencies) : ExecutionStrategy(dependencies, 1, TimeSpan.Zero)
  {
    protected override bool ShouldRetryOn(Exception exception) => exception is TimeoutException;
  }

  private sealed class FailOnceInterceptor : SaveChangesInterceptor
  {
    public bool FailNext { get; set; }
    public int Failures { get; private set; }
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
      if (FailNext)
      {
        FailNext = false;
        Failures++;
        throw new TimeoutException("Falha transitória simulada após SaveChanges.");
      }
      return ValueTask.FromResult(result);
    }
  }

  private static AppDbContext Context(SqliteConnection connection, FailOnceInterceptor interceptor) => new(
      new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection)
        .ReplaceService<IExecutionStrategyFactory, RetryFactory>().AddInterceptors(interceptor).Options);

  [Fact]
  public async Task Usuario_DeveReexecutarTransacaoSemDuplicarUsuarioPerfilOuLogin()
  {
    await using var connection = new SqliteConnection("DataSource=:memory:"); await connection.OpenAsync();
    var failure = new FailOnceInterceptor();
    await using var context = Context(connection, failure); await context.Database.EnsureCreatedAsync();
    failure.FailNext = true;
    var service = new UserService(context, Mock.Of<IS3Service>());
    var result = await service.CreateDefaultAsync(new("retry@example.com", "password123"), new("Nome"), new(ExternalLoginProvider.Google, "sub"));
    Assert.Equal(1, failure.Failures);
    Assert.Equal(1, await context.Users.CountAsync()); Assert.Equal(1, await context.Profiles.CountAsync());
    Assert.Equal(result.Id, (await context.ExternalLogins.SingleAsync()).UserId);
  }

  [Fact]
  public async Task Ponto_DeveReexecutarELimparUploadDaTentativaRevertida()
  {
    await using var connection = new SqliteConnection("DataSource=:memory:"); await connection.OpenAsync();
    var failure = new FailOnceInterceptor();
    await using var context = Context(connection, failure); await context.Database.EnsureCreatedAsync();
    var collector = new Collector { Id = Guid.NewGuid(), Name = "Papel" };
    context.Collectors.Add(collector); await context.SaveChangesAsync();
    var geocoder = new Mock<INominatimService>();
    geocoder.Setup(n => n.ReverseAsync(1, 2)).ReturnsAsync(new CreateAddressDto("Rua", "", "", "", "", "", "", 1, 2));
    var uploads = new List<string>();
    var s3 = new Mock<IS3Service>();
    s3.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), "image/png", default))
      .Returns<Stream, string, string, CancellationToken>((_, key, _, _) => { uploads.Add(key); return Task.FromResult("https://example.com/" + key); });
    s3.Setup(s => s.DeleteAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);
    failure.FailNext = true;
    var png = new FormFile(new MemoryStream([137,80,78,71,13,10,26,10]), 0, 8, "Images", "photo.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };
    var result = await new CollectionPointService(context, geocoder.Object, s3.Object).CreateAsync(
      new CreateCollectionPointDto { Lat = 1, Lng = 2, CollectorIds = [collector.Id], Images = [png] }, ["image"]);
    Assert.Equal(1, failure.Failures); Assert.Equal(2, uploads.Count);
    s3.Verify(s => s.DeleteAsync(uploads[0], default), Times.Once);
    Assert.Equal(1, await context.CollectionPoints.CountAsync()); Assert.Equal(1, await context.Images.CountAsync());
    Assert.Equal("https://example.com/" + uploads[1], Assert.Single(result.Images!).StoragePath);
  }

  [Fact]
  public async Task Google_DeveRecarregarUsuarioEPerfilAntesDeRepetirVinculo()
  {
    await using var connection = new SqliteConnection("DataSource=:memory:"); await connection.OpenAsync();
    var failure = new FailOnceInterceptor();
    await using var context = Context(connection, failure); await context.Database.EnsureCreatedAsync();
    var user = new User { Id = Guid.NewGuid(), Email = "retry@gmail.com", IsActived = true };
    context.Users.Add(user); await context.SaveChangesAsync();
    var google = new Mock<IGoogleTokenValidator>();
    google.Setup(g => g.ValidateAsync("id-token")).ReturnsAsync(new GoogleJsonWebSignature.Payload { Subject = "google-sub", Email = user.Email, EmailVerified = true, Name = "Nome" });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
      ["Jwt:Key"] = "test-key-with-more-than-32-characters-for-signing-tokens",
      ["Jwt:Issuer"] = "test", ["Jwt:Audience"] = "test", ["Jwt:AccessTokenExpirationMinutes"] = "15", ["Jwt:RefreshTokenExpirationDays"] = "7"
    }).Build();
    failure.FailNext = true;
    var result = await new AuthService(context, config, new UserService(context, Mock.Of<IS3Service>()), google.Object).GoogleLoginAsync(new("id-token"));
    Assert.NotEmpty(result.AccessToken); Assert.Equal(1, failure.Failures);
    Assert.Equal(1, await context.ExternalLogins.CountAsync()); Assert.Equal(1, await context.Profiles.CountAsync());
    Assert.Equal(1, await context.RefreshTokens.CountAsync());
  }
}
