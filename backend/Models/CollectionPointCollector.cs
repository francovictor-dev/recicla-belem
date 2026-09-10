namespace Backend.Models;

public class CollectionPointCollector
{
  public Guid Id { get; set; }
  public Guid CollectionPointId { get; set; }
  public Guid CollectorId { get; set; }
  public CollectionPoint CollectionPoint { get; set; } = null!;
  public Collector Collector { get; set; } = null!;
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}