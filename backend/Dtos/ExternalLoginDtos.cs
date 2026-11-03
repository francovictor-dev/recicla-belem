using Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public record CreateExternalLoginDto([EnumDataType(typeof(ExternalLoginProvider))] ExternalLoginProvider Provider, [Required] string ProviderId, Guid UserId);
public record UpdateExternalLoginDto([EnumDataType(typeof(ExternalLoginProvider))] ExternalLoginProvider Provider, [Required] string ProviderId, Guid UserId);
public record ExternalLoginResponseDto(Guid Id, ExternalLoginProvider Provider, string ProviderId, Guid UserId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
