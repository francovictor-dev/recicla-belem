using Backend.Dtos;
using Backend.Data;
using Backend.Models;
using Backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CollectionPointService : ICollectionPointService
{
  private readonly AppDbContext _context;
  private readonly INominatimService _nominatim;
  private readonly IS3Service _s3;

  public CollectionPointService(AppDbContext context, INominatimService nominatim, IS3Service s3)
  {
    _context = context;
    _nominatim = nominatim;
    _s3 = s3;
  }

  public async Task<CollectionPointResponseDto> CreateAsync(CreateCollectionPointDto dto, IEnumerable<string> include)
  {
    if (!double.IsFinite(dto.Lat) || !double.IsFinite(dto.Lng) || dto.Lat is < -90 or > 90 || dto.Lng is < -180 or > 180)
      throw new ArgumentException("Coordenadas inválidas.");
    if (dto.CollectorIds.Count == 0) throw new ArgumentException("Selecione pelo menos um coletor.");
    if (dto.Images.Count is < 1 or > 10) throw new ArgumentException("Envie entre uma e dez imagens PNG.");
    foreach (var file in dto.Images)
    {
      if (file.Length is <= 0 or > 5 * 1024 * 1024 || file.ContentType != "image/png")
        throw new ArgumentException("Cada imagem deve ser PNG e ter até 5 MB.");
      await using var stream = file.OpenReadStream();
      var signature = new byte[8];
      var read = await stream.ReadAtLeastAsync(signature, 8, false);
      if (read != 8 || !signature.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        throw new ArgumentException("Arquivo PNG inválido.");
    }

    var strategy = _context.Database.CreateExecutionStrategy();
    var firstAttempt = true;
    return await strategy.ExecuteAsync(async () =>
    {
      if (!firstAttempt) _context.ChangeTracker.Clear();
      firstAttempt = false;
      await using var transaction = await _context.Database.BeginTransactionAsync();
      var uploaded = new List<string>();
      try
      {
        var resolved = await _nominatim.ReverseAsync(dto.Lat, dto.Lng);
        var collectorIds = dto.CollectorIds.Distinct().ToList();
        var collectors = await _context.Collectors.Where(c => collectorIds.Contains(c.Id)).ToListAsync();
        if (collectors.Count != collectorIds.Count) throw new KeyNotFoundException("Um ou mais coletores não foram encontrados.");
        var now = DateTimeOffset.UtcNow;
        var point = new CollectionPoint
        {
          Id = Guid.NewGuid(), IsActive = true, Status = CollectionPointStatus.Approved,
          CreatedAt = now, UpdatedAt = now,
          Address = new Address
          {
            Id = Guid.NewGuid(), Road = resolved.Road, Number = resolved.Number, Complement = resolved.Complement,
            Neighborhood = resolved.Neighborhood, City = resolved.City, State = resolved.State, ZipCode = resolved.ZipCode,
            Lat = dto.Lat, Lng = dto.Lng, CreatedAt = now, UpdatedAt = now
          }
        };
        point.AddressId = point.Address.Id;
        foreach (var collector in collectors)
          point.CollectionPointCollectors.Add(new CollectionPointCollector
          {
            Id = Guid.NewGuid(), CollectionPoint = point, Collector = collector,
            CollectionPointId = point.Id, CollectorId = collector.Id, CreatedAt = now, UpdatedAt = now
          });
        foreach (var file in dto.Images)
        {
          var imageId = Guid.NewGuid();
          var key = $"images/{point.Id}/{imageId}.png";
          await using var stream = file.OpenReadStream();
          var path = await _s3.UploadAsync(stream, key, "image/png");
          uploaded.Add(key);
          point.Images.Add(new Image { Id = imageId, StoragePath = path, CollectionPointId = point.Id, CreatedAt = now, UpdatedAt = now });
        }
        _context.CollectionPoints.Add(point);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return MapToDto(point, include);
      }
      catch (Exception original)
      {
        var errors = new List<Exception> { original };
        foreach (var key in uploaded)
        {
          try { await _s3.DeleteAsync(key); }
          catch (Exception cleanupError) { errors.Add(cleanupError); }
        }
        if (errors.Count > 1) throw new AggregateException("Falha no cadastro e na remoção de imagens do S3.", errors);
        throw;
      }
    });
  }

  public async Task<CollectionPointResponseDto?> GetByIdAsync(Guid id, IEnumerable<string> include)
  {
    IQueryable<CollectionPoint> query = _context.CollectionPoints;
    var includes = include.ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (includes.Contains("address"))
      query = query.Include(cp => cp.Address);

    if (includes.Contains("collector"))
      query = query.Include(cp => cp.CollectionPointCollectors).ThenInclude(cpc => cpc.Collector);

    if (includes.Contains("image"))
      query = query.Include(cp => cp.Images);

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
    if (includes.Contains("image"))
      query = query.Include(cp => cp.Images);

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
    if (includes.Contains("image"))
      query = query.Include(cp => cp.Images);

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
        !includes.Contains("collector") ? null : collectionPoint.CollectionPointCollectors.Select(cpc => new CollectorResponseDto(cpc.Collector.Id, cpc.Collector.Name, cpc.Collector.Description, cpc.Collector.Color, cpc.Collector.CreatedAt, cpc.Collector.UpdatedAt)),
        !includes.Contains("image") ? null : collectionPoint.Images.Select(image => new ImageResponseDto(image.Id, image.StoragePath, image.CollectionPointId, image.CreatedAt, image.UpdatedAt)));
  }
}
