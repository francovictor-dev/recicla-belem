using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Backend.Controllers;
using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CreateDefaultControllerTests
{
  [Fact]
  public void Swagger_DeveDocumentarCadastroJsonEFormularioSemConflito()
  {
    using var factory = new CustomWebApplicationFactory();
    using var scope = factory.Services.CreateScope();
    var document = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

    var multipart = document.Paths["/api/User/default"].Operations[Microsoft.OpenApi.Models.OperationType.Post];
    var json = document.Paths["/api/User/default/json"].Operations[Microsoft.OpenApi.Models.OperationType.Post];
    Assert.Contains("multipart/form-data", multipart.RequestBody.Content.Keys);
    Assert.Contains("application/json", json.RequestBody.Content.Keys);
  }

  [Fact]
  public async Task Multipart_DeveRetornar400QuandoServicoRejeitaImagem()
  {
    var service = new Mock<IUserService>();
    var dto = new CreateDefaultUserFormDto { User = new("test@example.com", "password123"), Profile = new("Nome") };
    service.Setup(s => s.CreateDefaultAsync(dto.User, dto.Profile, null, null)).ThrowsAsync(new ArgumentException("Imagem inválida"));
    var result = await new UserController(service.Object).CreateDefaultWithImage(dto);
    Assert.IsType<BadRequestObjectResult>(result);
  }

  [Fact]
  public async Task Json_DeveEncaminharLoginExterno()
  {
    var service = new Mock<IUserService>();
    var external = new DefaultExternalLoginDto(Backend.Enums.ExternalLoginProvider.Google, "id");
    var dto = new CreateDefaultUserDto(new("test@example.com", "password123"), new("Nome"), external);
    service.Setup(s => s.CreateDefaultAsync(dto.User, dto.Profile, external, null))
      .ReturnsAsync(new UserResponseDto(Guid.NewGuid(), dto.User.Email, true, Backend.Enums.UserType.Email,
        backend.Enums.UserRole.Default, DateTimeOffset.UtcNow, default));
    Assert.IsType<CreatedAtActionResult>(await new UserController(service.Object).CreateDefault(dto));
    service.VerifyAll();
  }
}
