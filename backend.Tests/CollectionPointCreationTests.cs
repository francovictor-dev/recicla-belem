using Backend.Data;
using Backend.Dtos;
using Backend.Enums;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CollectionPointCreationTests
{
  private static IFormFile Png() => new FormFile(new MemoryStream([137,80,78,71,13,10,26,10]), 0, 8, "Images", "photo.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task CreateAsync_DeveCriarGrafoOuReverterERemoverUploads(bool fail)
  {
    await using var connection = new SqliteConnection("DataSource=:memory:");
    await connection.OpenAsync();
    await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
    await context.Database.EnsureCreatedAsync();
    var collector = new Collector { Id = Guid.NewGuid(), Name = "Papel" };
    context.Collectors.Add(collector); await context.SaveChangesAsync();
    var nominatim = new Mock<INominatimService>();
    nominatim.Setup(n => n.ReverseAsync(1, 2)).ReturnsAsync(new CreateAddressDto("Rua", "12", "", "Centro", "Belém", "Pará", "66000", 1, 2));
    var s3 = new Mock<IS3Service>();
    var keys = new List<string>();
    s3.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), "image/png", default))
      .Returns<Stream, string, string, CancellationToken>((_, key, _, _) => {
        if (fail && keys.Count == 1) throw new IOException("Upload falhou");
        keys.Add(key); return Task.FromResult("https://example.com/" + key);
      });
    s3.Setup(s => s.DeleteAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);
    var service = new CollectionPointService(context, nominatim.Object, s3.Object);
    var dto = new CreateCollectionPointDto { Lat = 1, Lng = 2, CollectorIds = [collector.Id, collector.Id], Images = [Png(), Png()] };
    if (fail)
    {
      await Assert.ThrowsAsync<IOException>(() => service.CreateAsync(dto, ["address", "collector", "image"]));
      context.ChangeTracker.Clear();
      Assert.Empty(await context.CollectionPoints.ToListAsync());
      Assert.Empty(await context.Addresses.ToListAsync());
      Assert.Empty(await context.Images.ToListAsync());
      s3.Verify(s => s.DeleteAsync(keys[0], default), Times.Once);
    }
    else
    {
      var result = await service.CreateAsync(dto, ["address", "collector", "image"]);
      Assert.True(result.IsActive); Assert.Equal(CollectionPointStatus.Approved, result.Status);
      Assert.Equal("Rua", result.Address!.Road); Assert.Single(result.Collectors!);
      Assert.Equal(2, result.Images!.Count());
      foreach (var image in result.Images!)
        Assert.Equal($"https://example.com/images/{result.Id}/{image.Id}.png", image.StoragePath);
    }
  }
}
