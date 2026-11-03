using Backend.Enums;
using Backend.Filters;

namespace Backend.Models;

public class ExternalLogin : IOwnedEntity
{
  public Guid Id { get; set; }
  public ExternalLoginProvider Provider { get; set; }
  public string ProviderId { get; set; } = "";
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
  public User User { get; set; } = null!;
  public Guid UserId { get; set; }
}
