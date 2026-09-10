using Backend.Enums;
namespace Backend.Dtos;

public record CreateCollectionPointDto(CreateCollectionPointDataDto CollectionPoint, CreateAddressDto Address, IEnumerable<Guid> CollectorIds);
public record CreateCollectionPointDataDto(bool IsActive, CollectionPointStatus Status);
public record UpdateCollectionPointDto(bool IsActived, CollectionPointStatus Status);
public record CollectionPointResponseDto(Guid Id, bool IsActive, Guid AddressId, CollectionPointStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, AddressResponseDto? Address = null, IEnumerable<CollectorResponseDto>? Collectors = null);