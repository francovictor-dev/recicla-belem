using Backend.Dtos;
using Backend.Services;

public interface IProfileService
{
    Task<ProfileResponseDto> CreateAsync(Guid userId, CreateProfileDto dto);
    Task<ProfileResponseDto?> GetByIdAsync(Guid id);
    Task<ProfileResponseDto> UpdateAsync(Guid id, UpdateProfileDto dto);
    Task DeleteAsync(Guid id);
}