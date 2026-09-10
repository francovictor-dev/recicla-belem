// Backend/Dtos/ProfileDtos.cs
namespace Backend.Dtos;

public record CreateProfileDto(string Name);
public record UpdateProfileDto(string Name);
public record ProfileResponseDto(Guid Id, string Name, Guid UserId);