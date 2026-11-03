using Backend.Data;
using Backend.Data.Seeders;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests;

public class CollectorSeederTests
{
  private static AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

  [Fact]
  public async Task SeedAsync_DeveCriarSeisColetoresQuandoTabelaVazia()
  {
    await using var context = CreateContext();
    await CollectorSeeder.SeedAsync(context);
    var collectors = await context.Collectors.ToListAsync();
    Assert.Equal(6, collectors.Count);
    Assert.Contains(collectors, c => c.Name == "Papel e Papelão" && c.Color == "#5B8FD9" && c.Description == "Caixas de papelão ondulado, embalagens de sapato e caixas de transporte em geral.");
    Assert.Contains(collectors, c => c.Name == "Plásticos" && c.Color == "#E57373" && c.Description == "Garrafas de refrigerante, água, óleo de cozinha e recipientes plásticos transparentes.");
    Assert.Contains(collectors, c => c.Name == "Vidro" && c.Color == "#66A879" && c.Description == "Recipientes de cerveja, vinho, refrigerantes e sucos");
    Assert.Contains(collectors, c => c.Name == "Metais" && c.Color == "#E8C65A" && c.Description == "Latinhas de cerveja e refrigerante, marmitas descartáveis e esquadrias.");
    Assert.Contains(collectors, c => c.Name == "Orgânicos" && c.Color == "#A8795E" && !string.IsNullOrWhiteSpace(c.Description));
    Assert.Contains(collectors, c => c.Name == "Não recicláveis" && c.Color == "#9CA3AF" && !string.IsNullOrWhiteSpace(c.Description));
    Assert.All(collectors, c =>
    {
      Assert.NotEqual(Guid.Empty, c.Id);
      Assert.NotEqual(default, c.CreatedAt);
      Assert.Equal(c.CreatedAt, c.UpdatedAt);
    });
    Assert.Equal(6, collectors.Select(c => c.Id).Distinct().Count());
  }

  [Fact]
  public async Task SeedAsync_DeveAdicionarPadroesSemModificarColetorPersonalizado()
  {
    await using var context = CreateContext();
    var existing = new Collector { Id = Guid.NewGuid(), Name = "Existente", Color = "#123456", Description = "Manter" };
    context.Collectors.Add(existing);
    await context.SaveChangesAsync();
    await CollectorSeeder.SeedAsync(context);
    Assert.Equal(7, await context.Collectors.CountAsync());
    var result = await context.Collectors.SingleAsync(c => c.Id == existing.Id);
    Assert.Equal(existing.Id, result.Id);
    Assert.Equal("Existente", result.Name);
    Assert.Equal("#123456", result.Color);
    Assert.Equal("Manter", result.Description);
    Assert.Equal(default, result.UpdatedAt);
  }

  [Fact]
  public async Task SeedAsync_NaoDeveDuplicarAoExecutarNovamente()
  {
    await using var context = CreateContext();
    await CollectorSeeder.SeedAsync(context);
    var originalIds = await context.Collectors.Select(c => c.Id).ToListAsync();
    await CollectorSeeder.SeedAsync(context);
    Assert.Equal(6, await context.Collectors.CountAsync());
    Assert.All(await context.Collectors.ToListAsync(), c => Assert.Contains(c.Id, originalIds));
  }

  [Fact]
  public async Task SeedAsync_DeveAtualizarCoresAntigasEPreservarId()
  {
    await using var context = CreateContext();
    var id = Guid.NewGuid();
    var created = DateTimeOffset.UtcNow.AddDays(-1);
    context.Collectors.Add(new Collector { Id = id, Name = "Papel e Papelão", Color = "#0000FF", Description = "Antiga", CreatedAt = created });
    await context.SaveChangesAsync();
    await CollectorSeeder.SeedAsync(context);
    Assert.Equal(6, await context.Collectors.CountAsync());
    var result = await context.Collectors.SingleAsync(c => c.Id == id);
    Assert.Equal("#5B8FD9", result.Color);
    Assert.Equal(created, result.CreatedAt);
    Assert.True(result.UpdatedAt > created);
    var timestamp = result.UpdatedAt;
    await CollectorSeeder.SeedAsync(context);
    Assert.Equal(timestamp, result.UpdatedAt);
  }

  [Fact]
  public async Task Inicializacao_DeveExecutarSeederAutomaticamente()
  {
    using var factory = new CustomWebApplicationFactory();
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Assert.Equal(6, await context.Collectors.CountAsync());
  }
}
