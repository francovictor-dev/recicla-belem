namespace Backend.Dtos;

public record CreateCollectionPointCollectorDto(Guid CollectionPointId, Guid CollectorId);
public record UpdateCollectionPointCollectorDto(Guid CollectionPointId, Guid CollectorId);
public record CollectionPointCollectorResponseDto(Guid Id, Guid CollectionPointId, Guid CollectorId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);