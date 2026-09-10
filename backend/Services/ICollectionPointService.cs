using Backend.Dtos;
using Backend.Services;

public interface ICollectionPointService
{
  Task<CollectionPointResponseDto> CreateAsync(CreateCollectionPointDto dto);
  Task<CollectionPointResponseDto?> GetByIdAsync(Guid id, IEnumerable<string> include);
  Task<IEnumerable<CollectionPointResponseDto>> GetAllAsync(IEnumerable<string> include);
  Task<CollectionPointResponseDto> UpdateAsync(Guid id, UpdateCollectionPointDto dto, IEnumerable<string> include);
  Task DeleteAsync(Guid id);
}