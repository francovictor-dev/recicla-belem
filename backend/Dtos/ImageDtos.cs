namespace Backend.Dtos;

public record CreateImageDto(string StoragePath, Guid CollectionPointId);
public record UpdateImageDto(string StoragePath);
public record ImageResponseDto(Guid Id, string StoragePath, Guid CollectionPointId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);