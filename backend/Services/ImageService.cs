using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ImageService : IImageService
{
  private readonly AppDbContext _context;

  public ImageService(AppDbContext context)
  {
    _context = context;
  }

  public async Task<ImageResponseDto> CreateAsync(CreateImageDto dto)
  {
    if (!await _context.CollectionPoints.AnyAsync(collectionPoint => collectionPoint.Id == dto.CollectionPointId))
      throw new KeyNotFoundException("Ponto de coleta não encontrado.");

    var now = DateTimeOffset.UtcNow;
    var image = new Image
    {
      Id = Guid.NewGuid(),
      StoragePath = dto.StoragePath,
      CollectionPointId = dto.CollectionPointId,
      CreatedAt = now,
      UpdatedAt = now
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return MapToDto(image);
  }

  public async Task<ImageResponseDto?> GetByIdAsync(Guid id)
  {
    var image = await _context.Images.FindAsync(id);
    return image is null ? null : MapToDto(image);
  }

  public async Task<IEnumerable<ImageResponseDto>> GetAllAsync()
  {
    var images = await _context.Images.ToListAsync();
    return images.Select(MapToDto);
  }

  public async Task<ImageResponseDto> UpdateAsync(Guid id, UpdateImageDto dto)
  {
    var image = await _context.Images.FindAsync(id)
        ?? throw new KeyNotFoundException("Imagem não encontrada.");

    image.StoragePath = dto.StoragePath;
    image.UpdatedAt = DateTimeOffset.UtcNow;
    await _context.SaveChangesAsync();
    return MapToDto(image);
  }

  public async Task DeleteAsync(Guid id)
  {
    var image = await _context.Images.FindAsync(id)
        ?? throw new KeyNotFoundException("Imagem não encontrada.");

    _context.Images.Remove(image);
    await _context.SaveChangesAsync();
  }

  private static ImageResponseDto MapToDto(Image image) =>
      new(image.Id, image.StoragePath, image.CollectionPointId, image.CreatedAt, image.UpdatedAt);
}