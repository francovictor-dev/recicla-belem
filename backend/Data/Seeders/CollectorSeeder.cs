using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class CollectorSeeder
{
  public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default, ILogger? logger = null)
  {
    var now = DateTimeOffset.UtcNow;
    var defaults = new[]
    {
      Create("Papel e Papelão", "#5B8FD9", "Caixas de papelão ondulado, embalagens de sapato e caixas de transporte em geral.", now),
      Create("Plásticos", "#E57373", "Garrafas de refrigerante, água, óleo de cozinha e recipientes plásticos transparentes.", now),
      Create("Vidro", "#66A879", "Recipientes de cerveja, vinho, refrigerantes e sucos", now),
      Create("Metais", "#E8C65A", "Latinhas de cerveja e refrigerante, marmitas descartáveis e esquadrias.", now),
      Create("Orgânicos", "#A8795E", "Restos de alimentos, cascas de frutas e verduras e outros resíduos orgânicos.", now),
      Create("Não recicláveis", "#9CA3AF", "Resíduos gerais não recicláveis, como papel higiênico, fraldas e materiais contaminados.", now)
    };

    var existing = await context.Collectors.ToListAsync(cancellationToken);
    var added = 0;
    var updated = 0;
    foreach (var item in defaults)
    {
      var matches = existing.Where(collector => string.Equals(collector.Name.Trim(), item.Name, StringComparison.OrdinalIgnoreCase)).ToList();
      if (matches.Count == 0)
      {
        context.Collectors.Add(item);
        added++;
        continue;
      }
      foreach (var collector in matches)
      {
        if (collector.Color == item.Color && collector.Description == item.Description) continue;
        collector.Color = item.Color;
        collector.Description = item.Description;
        collector.UpdatedAt = now;
        updated++;
      }
    }
    if (added > 0 || updated > 0) await context.SaveChangesAsync(cancellationToken);
    logger?.LogInformation("Seeder de coletores: {Added} adicionados, {Updated} atualizados.", added, updated);
  }

  private static Collector Create(string name, string color, string description, DateTimeOffset now) => new()
  {
    Id = Guid.NewGuid(),
    Name = name,
    Color = color,
    Description = description,
    CreatedAt = now,
    UpdatedAt = now
  };
}
