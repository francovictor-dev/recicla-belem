namespace Backend.Models;

public class Image
{
  public Guid Id { get; set; }
  public string StoragePath { get; set; } = "";
  public Guid CollectionPointId { get; set; }
  public CollectionPoint CollectionPoint { get; set; } = null!;
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}