using Backend.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Backend.Enums;
namespace Backend.Dtos;

public class CreateCollectionPointDto
{
  [BindRequired]
  [ModelBinder(BinderType = typeof(InvariantDoubleModelBinder))]
  [System.ComponentModel.DataAnnotations.Range(-90.0, 90.0)]
  public double Lat { get; set; }
  [BindRequired]
  [ModelBinder(BinderType = typeof(InvariantDoubleModelBinder))]
  [System.ComponentModel.DataAnnotations.Range(-180.0, 180.0)]
  public double Lng { get; set; }
  public List<Guid> CollectorIds { get; set; } = [];
  public List<IFormFile> Images { get; set; } = [];
}
public record UpdateCollectionPointDto(bool IsActived, CollectionPointStatus Status);
public record CollectionPointResponseDto(Guid Id, bool IsActive, Guid AddressId, CollectionPointStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, AddressResponseDto? Address = null, IEnumerable<CollectorResponseDto>? Collectors = null, IEnumerable<ImageResponseDto>? Images = null);