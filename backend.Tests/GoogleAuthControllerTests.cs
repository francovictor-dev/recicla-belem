using Backend.Controllers;
using Backend.Configurations;
using Backend.Data;
using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Backend.Tests;

public class GoogleAuthControllerTests
{
  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public async Task GoogleLogin_DeveRetornarTokensOu401(bool accepted)
  {
    await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var auth = new Mock<IAuthService>();
    if (accepted) auth.Setup(a => a.GoogleLoginAsync(It.IsAny<GoogleLoginDto>())).ReturnsAsync(new AuthResponseDto("jwt", "refresh", DateTimeOffset.UtcNow));
    else auth.Setup(a => a.GoogleLoginAsync(It.IsAny<GoogleLoginDto>())).ThrowsAsync(new UnauthorizedAccessException("Token inválido"));
    var result = await new AuthController(auth.Object, context, Options.Create(new S3Options())).GoogleLogin(new("id-token"));
    if (accepted) Assert.IsType<OkObjectResult>(result); else Assert.IsType<UnauthorizedObjectResult>(result);
  }
}
