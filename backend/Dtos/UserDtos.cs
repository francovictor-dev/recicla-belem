namespace Backend.Dtos;

using backend.Enums;
using Backend.Enums;

public record CreateUserDto(string Email, string Password, UserType UserType = UserType.Email);
public record DefaultExternalLoginDto(ExternalLoginProvider Provider, string ProviderId);
public record CreateDefaultUserDto(CreateUserDto User, CreateProfileDto Profile, DefaultExternalLoginDto? ExternalLogin = null);
public class CreateDefaultUserFormDto
{
  public CreateUserDto User { get; set; } = null!;
  public CreateProfileDto Profile { get; set; } = null!;
  public DefaultExternalLoginDto? ExternalLogin { get; set; }
  public IFormFile? Image { get; set; }
}
public record UpdateUserDto(string Email, bool IsActived, UserType UserType = UserType.Email);
public record UserResponseDto(Guid Id, string Email, bool IsActived, UserType UserType, UserRole UserRole, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
