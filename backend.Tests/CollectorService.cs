using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentAssertions;
using Backend.Services;
using Xunit;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace Backend.Tests;
public class CollectorServiceTests
{
  private static AppDbContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isola cada teste
          .Options;
    return new AppDbContext(options);
  }

  [Fact]
  public async Task CreateAsync_DeveCriarColetor_QuandoDadosValidos()
  {
    await using var context = CreateContext();
    var service = new CollectorService(context);
    var dto = new CreateCollectorDto("Coletor Teste", "Descrição do coletor teste", "#FF5733");

    var result = await service.CreateAsync(dto);

    result.Should().NotBeNull();
    result.Name.Should().Be("Coletor Teste");
    result.Description.Should().Be("Descrição do coletor teste");
    result.Color.Should().Be("#FF5733");
    result.Id.Should().NotBeEmpty();

    var collectorInDb = await context.Collectors.FindAsync(result.Id);
    collectorInDb.Should().NotBeNull();
  }
}