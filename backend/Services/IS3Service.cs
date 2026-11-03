namespace Backend.Services;

public interface IS3Service
{
  Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);
  Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
