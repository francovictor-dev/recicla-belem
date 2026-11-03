// Backend/Dtos/ProfileDtos.cs
namespace Backend.Dtos;

public record CreateProfileDto(string Name, string? PhotoUrl = null);
public record UpdateProfileDto(string Name);
public record ProfileResponseDto(Guid Id, string Name, Guid UserId, string? PhotoUrl = null);