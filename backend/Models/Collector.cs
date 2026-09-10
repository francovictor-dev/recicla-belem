namespace Backend.Models;
public class Collector
{
  public Guid Id { get; set; }
  public String Name { get; set; } = "";
  public String Description { get; set; } = "";
  public String Color { get; set; } = "";
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}