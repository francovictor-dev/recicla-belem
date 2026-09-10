using backend.Enums;
using Backend.Enums;
using Backend.Filters;
namespace Backend.Models;
public class User : IOwnedEntity
{
  public Guid Id { get; set; }
  public String Email { get; set; } = "";
  public bool IsActived {get;set;} = true;
  public UserType UserType { get; set; } = UserType.Email;
  public UserRole UserRole { get; set; } = UserRole.Default;
  public String Password { get; set; } = "";
  public Profile? Profile { get; set; }
  public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
  Guid IOwnedEntity.UserId => Id;
}