using Backend.Dtos;

namespace Backend.Services;

public interface IExternalLoginService : ICrudService<CreateExternalLoginDto, UpdateExternalLoginDto, ExternalLoginResponseDto>
{
}
