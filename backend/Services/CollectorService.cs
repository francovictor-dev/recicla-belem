using Backend.Data;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Services;

public interface ICollectorService : ICrudService<CreateCollectorDto, UpdateCollectorDto, CollectorResponseDto>
{
}


public class CollectorService(AppDbContext context) : ICollectorService
{
  private readonly AppDbContext _context = context;

  public async Task<CollectorResponseDto> CreateAsync(CreateCollectorDto dto)
  {

    var collector = new Collector
    {
      Id = Guid.NewGuid(),
      Name = dto.Name,
      Description = dto.Description,
      Color = dto.Color,
      CreatedAt = DateTimeOffset.UtcNow,
      UpdatedAt = DateTimeOffset.UtcNow
    };

    _context.Collectors.Add(collector);
    await _context.SaveChangesAsync();
    return MapToDto(collector);
  }

  public async Task<CollectorResponseDto?> GetByIdAsync(Guid id)
  {
    var collector = await _context.Collectors.FindAsync(id);
    if (collector is null)
      return null;

    return MapToDto(collector);
  }
  public async Task<IEnumerable<CollectorResponseDto>> GetAllAsync()
  {
    var collectors = await _context.Collectors.ToListAsync();
    return collectors.Select(MapToDto);
  }
  public async Task<CollectorResponseDto> UpdateAsync(Guid id, UpdateCollectorDto dto)
  {
    var collector = await _context.Collectors.FindAsync(id) ?? throw new KeyNotFoundException("Coletor não encontrado.");
    collector.Name = dto.Name;
    collector.Description = dto.Description;
    collector.Color = dto.Color;
    collector.UpdatedAt = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();

    return MapToDto(collector);
  }
  public async Task DeleteAsync(Guid id)
  {
    var collector = await _context.Collectors.FindAsync(id) ?? throw new KeyNotFoundException("Coletor não encontrado.");
    _context.Collectors.Remove(collector);
    await _context.SaveChangesAsync();
  }
  private static CollectorResponseDto MapToDto(Collector collector)
      => new(collector.Id,collector.Name, collector.Description, collector.Color, collector.CreatedAt, collector.UpdatedAt);
}