namespace Backend.Models;
using Backend.Enums;
public class Report
{
  public Guid Id { get; set; }
  public String Message { get; set; } = "";
  public ReportType ReportType { get; set; } = ReportType.NotExist;
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
  public Guid CollectionPointId { get; set; }
  public CollectionPoint CollectionPoint { get; set; } = null!;
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}