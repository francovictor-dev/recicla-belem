namespace Backend.Dtos;

public record CreateCollectorDto(string Name, string Description, string Color);
public record UpdateCollectorDto(string Name, string Description, string Color);
public record CollectorResponseDto(Guid Id, string Name, string Description, string Color, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);