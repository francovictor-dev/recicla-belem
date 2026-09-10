namespace Backend.Models;

using Backend.Enums;
public class CollectionPoint
{
  public Guid Id { get; set; }
  public bool IsActive { get; set; } = true;
  public Guid AddressId { get; set; }
  public Address Address { get; set; } = null!;
  public ICollection<CollectionPointCollector> CollectionPointCollectors { get; set; } = new List<CollectionPointCollector>();
  public CollectionPointStatus Status { get; set; } = CollectionPointStatus.Pending;
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}