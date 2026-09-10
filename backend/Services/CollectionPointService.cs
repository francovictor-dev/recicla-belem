using Backend.Dtos;
using Backend.Data;
using Backend.Models;
using Backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CollectionPointService : ICollectionPointService
{
  private readonly AppDbContext _context;

  public CollectionPointService(AppDbContext context)
  {
    _context = context;
  }

  public async Task<CollectionPointResponseDto> CreateAsync(CreateCollectionPointDto dto)
  {
    var collectorIds = dto.CollectorIds.Distinct().ToList();
    var collectors = await _context.Collectors
      .Where(collector => collectorIds.Contains(collector.Id))
      .ToListAsync();

    var missingCollectorIds = collectorIds.Except(collectors.Select(collector => collector.Id)).ToList();
    if (missingCollectorIds.Count > 0)
      throw new KeyNotFoundException("Um ou mais coletores não foram encontrados.");

    var now = DateTimeOffset.UtcNow;
    var address = new Address
    {
      Id = Guid.NewGuid(),
      Road = dto.Address.Road,
      Number = dto.Address.Number,
      Complement = dto.Address.Complement,
      Neighborhood = dto.Address.Neighborhood,
      City = dto.Address.City,
      State = dto.Address.State,
      ZipCode = dto.Address.ZipCode,
      Lat = dto.Address.Lat,
      Lng = dto.Address.Lng,
      CreatedAt = now,
      UpdatedAt = now
    };

    _context.Addresses.Add(address);

    var collectionPoint = new CollectionPoint
    {
      Id = Guid.NewGuid(),
      AddressId = address.Id,
      Address = address,
      IsActive = dto.CollectionPoint.IsActive,
      Status = dto.CollectionPoint.Status,
      CreatedAt = now,
      UpdatedAt = now
    };

    _context.CollectionPoints.Add(collectionPoint);

    var collectionPointCollectors = collectors.Select(collector => new CollectionPointCollector
    {
      Id = Guid.NewGuid(),
      CollectionPointId = collectionPoint.Id,
      CollectorId = collector.Id,
      CollectionPoint = collectionPoint,
      Collector = collector,
      CreatedAt = now,
      UpdatedAt = now
    }).ToList();

    collectionPoint.CollectionPointCollectors = collectionPointCollectors;
    _context.CollectionPointCollectors.AddRange(collectionPointCollectors);

    await _context.SaveChangesAsync();

    return MapToDto(collectionPoint, new[] { "address", "collector" });
  }

  public async Task<CollectionPointResponseDto?> GetByIdAsync(Guid id, IEnumerable<string> include)
  {
    IQueryable<CollectionPoint> query = _context.CollectionPoints;
    var includes = include.ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (includes.Contains("address"))
      query = query.Include(cp => cp.Address);

    if (includes.Contains("collector"))
      query = query.Include(cp => cp.CollectionPointCollectors).ThenInclude(cpc => cpc.Collector);

    var collectionPoint = await query.FirstOrDefaultAsync(cp => cp.Id == id);

    return collectionPoint is null ? null : MapToDto(collectionPoint, includes);
    //var collectionPoint = await _context.CollectionPoints.FindAsync(id);
    //return collectionPoint is null ? null : MapToDto(collectionPoint);
  }
  public async Task<CollectionPointResponseDto> UpdateAsync(Guid id, UpdateCollectionPointDto dto, IEnumerable<string> include)
  {
    IQueryable<CollectionPoint> query = _context.CollectionPoints;
    var includes = include.ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (includes.Contains("address"))
      query = query.Include(cp => cp.Address);
    if (includes.Contains("collector"))
      query = query.Include(cp => cp.CollectionPointCollectors).ThenInclude(cpc => cpc.Collector);

    var collectionPoint = await query.FirstOrDefaultAsync(cp => cp.Id == id)
        ?? throw new KeyNotFoundException("Ponto de coleta não encontrado.");

    collectionPoint.IsActive = dto.IsActived;
    collectionPoint.Status = dto.Status;
    collectionPoint.UpdatedAt = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();
    return MapToDto(collectionPoint, includes);
  }
  public async Task DeleteAsync(Guid id)
  {
    var collectionPoint = await _context.CollectionPoints.FindAsync(id)
        ?? throw new KeyNotFoundException("Ponto de coleta não encontrado.");

    _context.CollectionPoints.Remove(collectionPoint);
    await _context.SaveChangesAsync();
  }
  public async Task<IEnumerable<CollectionPointResponseDto>> GetAllAsync(IEnumerable<string> include)
  {
    IQueryable<CollectionPoint> query = _context.CollectionPoints;
    var includes = include.ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (includes.Contains("address"))
      query = query.Include(cp => cp.Address);
    if (includes.Contains("collector"))
      query = query.Include(cp => cp.CollectionPointCollectors).ThenInclude(cpc => cpc.Collector);

    var collectionPoints = await query.ToListAsync();
    return collectionPoints.Select(v => MapToDto(v, includes));

    //var collectionPoints = await _context.CollectionPoints.ToListAsync();
    //return collectionPoints.Select(v => MapToDto(v, false));
  }
  public async Task<IEnumerable<CollectionPointResponseDto>> GetAllActiveAsync()
  {
    var collectionPoints = await _context.CollectionPoints
        .Where(cp => cp.IsActive)
        .ToListAsync();
    return collectionPoints.Select(v => MapToDto(v, Array.Empty<string>()));
  }
  private static CollectionPointResponseDto MapToDto(CollectionPoint collectionPoint, IEnumerable<string> include)
  {
    var includes = include.ToHashSet(StringComparer.OrdinalIgnoreCase);

    return new(
        collectionPoint.Id,
        collectionPoint.IsActive,
        collectionPoint.AddressId,
        collectionPoint.Status,
        collectionPoint.CreatedAt,
        collectionPoint.UpdatedAt,
        !includes.Contains("address") ? null : new AddressResponseDto(collectionPoint.Address.Id, collectionPoint.Address.Road, collectionPoint.Address.Number, collectionPoint.Address.Complement, collectionPoint.Address.Neighborhood, collectionPoint.Address.City, collectionPoint.Address.State, collectionPoint.Address.ZipCode, collectionPoint.Address.Lat, collectionPoint.Address.Lng, collectionPoint.Address.CreatedAt, collectionPoint.Address.UpdatedAt),
        !includes.Contains("collector") ? null : collectionPoint.CollectionPointCollectors.Select(cpc => new CollectorResponseDto(cpc.Collector.Id, cpc.Collector.Name, cpc.Collector.Description, cpc.Collector.Color, cpc.Collector.CreatedAt, cpc.Collector.UpdatedAt)));
  }
}
