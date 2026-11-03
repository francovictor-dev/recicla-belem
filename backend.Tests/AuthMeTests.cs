using System.Security.Claims;
using Backend.Controllers;
using Backend.Configurations;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Backend.Tests;

public class AuthMeTests
{
  [Fact]
  public async Task Me_DeveRetornarNomeECaminhoDaImagemDoUsuarioAutenticado()
  {
    await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var id = Guid.NewGuid();
    context.Users.Add(new User { Id = id, Email = "a@example.com", Profile = new Profile { Id = Guid.NewGuid(), UserId = id, Name = "Maria", PhotoUrl = "https://example.com/google-photo.jpg" } });
    await context.SaveChangesAsync();
    var controller = new AuthController(Mock.Of<IAuthService>(), context, Options.Create(new S3Options { PublicUrl = "https://example.com/" }));
    controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", id.ToString())], "test")) } };
    var result = Assert.IsType<OkObjectResult>(await controller.Me());
    var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
    Assert.Contains("Maria", json);
    Assert.Contains("https://example.com/google-photo.jpg", json);
  }

  [Fact]
  public async Task Me_DeveRejeitarIdentidadeSemIdValido()
  {
    await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var controller = new AuthController(Mock.Of<IAuthService>(), context, Options.Create(new S3Options()));
    controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    Assert.IsType<UnauthorizedResult>(await controller.Me());
  }
}
