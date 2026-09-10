using Backend.Filters;

namespace Backend.Models;

public class Profile : IOwnedEntity
{
  public Guid Id { get; set; }
  public String Name { get; set; } = "";
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
}
