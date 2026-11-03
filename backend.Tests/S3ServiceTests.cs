using Amazon.S3;
using Amazon.S3.Model;
using Backend.Configurations;
using Backend.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Backend.Tests;

public class S3ServiceTests
{
  private readonly Mock<IAmazonS3> _client = new(MockBehavior.Strict);
  private S3Service CreateService() => new(_client.Object, Options.Create(new S3Options
  {
    Region = "sa-east-1", Bucket = "test-bucket", PublicUrl = "https://example.com/"
  }));

  [Fact]
  public async Task UploadAsync_DeveEnviarArquivoERetornarUrlEscapada()
  {
    using var stream = new MemoryStream([1, 2, 3]);
    using var cancellation = new CancellationTokenSource();
    _client.Setup(client => client.PutObjectAsync(It.Is<PutObjectRequest>(request =>
      request.BucketName == "test-bucket" && request.Key == "profiles/foto teste.png" &&
      request.InputStream == stream && request.ContentType == "image/png" && !request.AutoCloseStream),
      cancellation.Token)).ReturnsAsync(new PutObjectResponse());

    var result = await CreateService().UploadAsync(stream, "profiles/foto teste.png", "image/png", cancellation.Token);

    Assert.Equal("https://example.com/profiles/foto%20teste.png", result);
    Assert.True(stream.CanRead);
    _client.VerifyAll();
  }

  [Fact]
  public async Task DeleteAsync_DeveExcluirPelaChaveEEncaminharCancelamento()
  {
    using var cancellation = new CancellationTokenSource();
    _client.Setup(client => client.DeleteObjectAsync(It.Is<DeleteObjectRequest>(request =>
      request.BucketName == "test-bucket" && request.Key == "profiles/foto.png"), cancellation.Token))
      .ReturnsAsync(new DeleteObjectResponse());
    await CreateService().DeleteAsync("profiles/foto.png", cancellation.Token);
    _client.VerifyAll();
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Operacoes_DevemRejeitarChavesVazias(string key)
  {
    using var stream = new MemoryStream([1]);
    var service = CreateService();
    await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(stream, key, "image/png"));
    await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(key));
    _client.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task UploadAsync_DeveRejeitarStreamFechado()
  {
    var stream = new MemoryStream();
    stream.Dispose();
    await Assert.ThrowsAsync<ArgumentException>(() => CreateService().UploadAsync(stream, "key", "image/png"));
    _client.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task UploadAsync_DevePropagarErroDaAws()
  {
    using var stream = new MemoryStream([1]);
    var error = new AmazonS3Exception("Access denied");
    _client.Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(error);
    var result = await Assert.ThrowsAsync<AmazonS3Exception>(() => CreateService().UploadAsync(stream, "key", "image/png"));
    Assert.Same(error, result);
  }

  [Fact]
  public async Task DeleteAsync_DevePropagarErroDaAws()
  {
    _client.Setup(client => client.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new AmazonS3Exception("Access denied"));
    await Assert.ThrowsAsync<AmazonS3Exception>(() => CreateService().DeleteAsync("key"));
  }
}
