namespace Backend.Services;


public interface ICrudService<TCreateDto, TUpdateDto, TResponseDto>
{
  Task<TResponseDto> CreateAsync(TCreateDto dto);
  Task<TResponseDto?> GetByIdAsync(Guid id);
  Task<IEnumerable<TResponseDto>> GetAllAsync();
  Task<TResponseDto> UpdateAsync(Guid id, TUpdateDto dto);
  Task DeleteAsync(Guid id);
}