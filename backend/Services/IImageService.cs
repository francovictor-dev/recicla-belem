using Backend.Dtos;

namespace Backend.Services;

public interface IImageService : ICrudService<CreateImageDto, UpdateImageDto, ImageResponseDto>
{
}