using Backend.Controllers;
using Backend.Dtos;
using Backend.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CollectionPointControllerTests
{
  [Fact]
  public async Task Create_DeveEncaminharFormularioEIncludesERetornar201()
  {
    var service = new Mock<ICollectionPointService>();
    var dto = new CreateCollectionPointDto { Lat = 1, Lng = 2 };
    var point = new CollectionPointResponseDto(Guid.NewGuid(), true, Guid.NewGuid(), CollectionPointStatus.Approved, DateTimeOffset.UtcNow, default);
    service.Setup(s => s.CreateAsync(dto, It.Is<IEnumerable<string>>(includes => includes.Contains("address") && includes.Contains("image"))))
      .ReturnsAsync(point);
    var result = Assert.IsType<CreatedAtActionResult>(await new CollectionPointController(service.Object).Create(dto, "address,image"));
    Assert.Same(point, result.Value);
    service.VerifyAll();
  }

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public async Task Create_DeveRetornarErroParaArquivoInvalidoOuColetorInexistente(bool invalidFile)
  {
    var service = new Mock<ICollectionPointService>();
    service.Setup(s => s.CreateAsync(It.IsAny<CreateCollectionPointDto>(), It.IsAny<IEnumerable<string>>()))
      .ThrowsAsync(invalidFile ? new ArgumentException("PNG inválido") : new KeyNotFoundException("Coletor não encontrado"));
    var result = await new CollectionPointController(service.Object).Create(new CreateCollectionPointDto());
    if (invalidFile) Assert.IsType<BadRequestObjectResult>(result);
    else Assert.IsType<NotFoundObjectResult>(result);
  }
}
