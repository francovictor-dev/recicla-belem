using System.Net;
using Backend.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Backend.Tests;

public class NominatimServiceTests
{
  private sealed class Handler(string json, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
  {
    public int Calls { get; private set; }
    public Uri? Uri { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
      Calls++; Uri = request.RequestUri;
      return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(json) });
    }
  }

  [Fact]
  public async Task ReverseAsync_DeveMapearEnderecoEReutilizarCache()
  {
    using var handler = new Handler("""{"address":{"road":"Rua", "house_number":"12", "suburb":"Centro", "town":"Belém", "state":"Pará", "postcode":"66000"}}""");
    using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
    using var cache = new MemoryCache(new MemoryCacheOptions());
    var service = new NominatimService(http, cache, NullLogger<NominatimService>.Instance);
    var result = await service.ReverseAsync(-1.5, -48.5);
    Assert.Equal("Rua", result.Road); Assert.Equal("12", result.Number);
    Assert.Equal("Centro", result.Neighborhood); Assert.Equal("Belém", result.City);
    Assert.Equal("Pará", result.State); Assert.Equal("66000", result.ZipCode);
    Assert.Equal("", result.Complement); Assert.Equal(-1.5, result.Lat);
    await service.ReverseAsync(-1.5, -48.5);
    Assert.Equal(1, handler.Calls);
    Assert.Contains("lat=-1.5", handler.Uri!.Query);
  }

  [Theory]
  [InlineData("{}", HttpStatusCode.OK)]
  [InlineData("invalid json", HttpStatusCode.OK)]
  [InlineData("{}", HttpStatusCode.ServiceUnavailable)]
  public async Task ReverseAsync_DeveManterCoordenadasEVaziosQuandoSemEndereco(string body, HttpStatusCode status)
  {
    using var handler = new Handler(body, status);
    using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
    using var cache = new MemoryCache(new MemoryCacheOptions());
    var result = await new NominatimService(http, cache, NullLogger<NominatimService>.Instance).ReverseAsync(1, 2);
    Assert.Equal("", result.Road); Assert.Equal("", result.Number); Assert.Equal("", result.City);
    Assert.Equal(1, result.Lat); Assert.Equal(2, result.Lng);
  }
}
