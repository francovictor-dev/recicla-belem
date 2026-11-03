using Backend.Dtos;

namespace Backend.Services;


public interface IUserService
{
  Task<UserResponseDto> CreateAsync(CreateUserDto dto);
  Task<UserResponseDto> CreateDefaultAsync(CreateUserDto dto, CreateProfileDto profileDto, DefaultExternalLoginDto? externalLoginDto = null, IFormFile? image = null);
  Task<UserResponseDto?> GetByIdAsync(Guid id);
  Task<IEnumerable<UserResponseDto>> GetAllAsync();
  Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto);
  Task DeleteAsync(Guid id);
}
