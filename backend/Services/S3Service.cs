using Amazon.S3;
using Amazon.S3.Model;
using Backend.Configurations;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class S3Service(IAmazonS3 client, IOptions<S3Options> options) : IS3Service
{
  private readonly IAmazonS3 _client = client;
  private readonly S3Options _options = options.Value;

  public async Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(content);
    ArgumentException.ThrowIfNullOrWhiteSpace(key);
    ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
    if (!content.CanRead)
      throw new ArgumentException("O arquivo deve permitir leitura.", nameof(content));

    await _client.PutObjectAsync(new PutObjectRequest
    {
      BucketName = _options.Bucket,
      Key = key,
      InputStream = content,
      ContentType = contentType,
      AutoCloseStream = false
    }, cancellationToken);

    var escapedKey = string.Join("/", key.Split('/').Select(Uri.EscapeDataString));
    return $"{_options.PublicUrl.TrimEnd('/')}/{escapedKey}";
  }

  public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key);
    await _client.DeleteObjectAsync(new DeleteObjectRequest
    {
      BucketName = _options.Bucket,
      Key = key
    }, cancellationToken);
  }
}
