using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CollectionPointCollectorService(AppDbContext context) : ICollectionPointCollectorService
{
    private readonly AppDbContext _context = context;

    public async Task<CollectionPointCollectorResponseDto> CreateAsync(CreateCollectionPointCollectorDto dto)
    {
        await ValidateReferencesAsync(dto.CollectionPointId, dto.CollectorId);

        var relationExists = await _context.CollectionPointCollectors
            .AnyAsync(cpc => cpc.CollectionPointId == dto.CollectionPointId && cpc.CollectorId == dto.CollectorId);

        if (relationExists)
            throw new InvalidOperationException("O coletor já está associado a este ponto de coleta.");

        var collectionPointCollector = new CollectionPointCollector
        {
            Id = Guid.NewGuid(),
            CollectionPointId = dto.CollectionPointId,
            CollectorId = dto.CollectorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.CollectionPointCollectors.Add(collectionPointCollector);
        await _context.SaveChangesAsync();

        return MapToDto(collectionPointCollector);
    }

    public async Task<CollectionPointCollectorResponseDto?> GetByIdAsync(Guid id)
    {
        var collectionPointCollector = await _context.CollectionPointCollectors.FindAsync(id);

        return collectionPointCollector is null ? null : MapToDto(collectionPointCollector);
    }

    public async Task<IEnumerable<CollectionPointCollectorResponseDto>> GetAllAsync()
    {
        var collectionPointCollectors = await _context.CollectionPointCollectors.ToListAsync();

        return collectionPointCollectors.Select(MapToDto);
    }

    public async Task<CollectionPointCollectorResponseDto> UpdateAsync(Guid id, UpdateCollectionPointCollectorDto dto)
    {
        await ValidateReferencesAsync(dto.CollectionPointId, dto.CollectorId);

        var collectionPointCollector = await _context.CollectionPointCollectors.FindAsync(id)
            ?? throw new KeyNotFoundException("Associação entre ponto de coleta e coletor não encontrada.");

        var relationExists = await _context.CollectionPointCollectors
            .AnyAsync(cpc => cpc.Id != id && cpc.CollectionPointId == dto.CollectionPointId && cpc.CollectorId == dto.CollectorId);

        if (relationExists)
            throw new InvalidOperationException("O coletor já está associado a este ponto de coleta.");

        collectionPointCollector.CollectionPointId = dto.CollectionPointId;
        collectionPointCollector.CollectorId = dto.CollectorId;
        collectionPointCollector.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(collectionPointCollector);
    }

    public async Task DeleteAsync(Guid id)
    {
        var collectionPointCollector = await _context.CollectionPointCollectors.FindAsync(id)
            ?? throw new KeyNotFoundException("Associação entre ponto de coleta e coletor não encontrada.");

        _context.CollectionPointCollectors.Remove(collectionPointCollector);
        await _context.SaveChangesAsync();
    }

    private async Task ValidateReferencesAsync(Guid collectionPointId, Guid collectorId)
    {
        var collectionPointExists = await _context.CollectionPoints.AnyAsync(cp => cp.Id == collectionPointId);
        if (!collectionPointExists)
            throw new KeyNotFoundException("Ponto de coleta não encontrado.");

        var collectorExists = await _context.Collectors.AnyAsync(c => c.Id == collectorId);
        if (!collectorExists)
            throw new KeyNotFoundException("Coletor não encontrado.");
    }

    private static CollectionPointCollectorResponseDto MapToDto(CollectionPointCollector collectionPointCollector)
        => new(collectionPointCollector.Id, collectionPointCollector.CollectionPointId, collectionPointCollector.CollectorId, collectionPointCollector.CreatedAt, collectionPointCollector.UpdatedAt);
}