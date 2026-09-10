namespace Backend.Models;
public class Address
{
  public Guid Id { get; set; }
  public String Road { get; set; } = "";
  public String Number { get; set; } = "";
  public String Complement { get; set; } = "";
  public String Neighborhood { get; set; } = "";
  public String City { get; set; } = "";
  public String State { get; set; } = "";
  public String ZipCode { get; set; } = "";
  public Double Lat { get; set; }
  public Double Lng { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}
