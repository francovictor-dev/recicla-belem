namespace Backend.Dtos;

using backend.Enums;
using Backend.Enums;

public record CreateUserDto(string Email, string Password, UserType UserType = UserType.Email);
public record UpdateUserDto(string Email, bool IsActived, UserType UserType = UserType.Email);
public record UserResponseDto(Guid Id, string Email, bool IsActived, UserType UserType, UserRole UserRole, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);