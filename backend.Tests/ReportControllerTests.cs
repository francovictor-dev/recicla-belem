using System.Security.Claims;
using Backend.Controllers;
using Backend.Dtos;
using Backend.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests;

public class ReportControllerTests
{
  [Fact]
  public async Task Create_DeveUsarUsuarioDoTokenEPontoDoFormulario()
  {
    var userId = Guid.NewGuid(); var pointId = Guid.NewGuid();
    var service = new Mock<IReportService>();
    service.Setup(s => s.CreateAsync(It.Is<CreateReportDto>(d => d.UserId == userId && d.CollectionPointId == pointId && d.Message == "Descrição")))
      .ReturnsAsync(new ReportResponseDto(Guid.NewGuid(), "Descrição", ReportType.NotExist, userId, pointId, DateTimeOffset.UtcNow, default));
    var controller = new ReportController(service.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "test")) } } };
    Assert.IsType<CreatedAtActionResult>(await controller.Create(new CreateReportDto("Descrição", ReportType.NotExist, Guid.NewGuid(), pointId)));
    service.VerifyAll();
  }

  [Fact]
  public async Task Create_DeveRejeitarTokenSemIdentificador()
  {
    var service = new Mock<IReportService>();
    var controller = new ReportController(service.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    Assert.IsType<UnauthorizedResult>(await controller.Create(new CreateReportDto("Descrição", ReportType.NotExist, Guid.NewGuid(), Guid.NewGuid())));
    service.VerifyNoOtherCalls();
  }
}
