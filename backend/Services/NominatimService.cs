using System.Globalization;
using System.Text.Json;
using Backend.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services;

public class NominatimService(HttpClient http, IMemoryCache cache, ILogger<NominatimService> logger) : INominatimService
{
  private readonly SemaphoreSlim _gate = new(1, 1);

  public async Task<CreateAddressDto> ReverseAsync(double lat, double lng)
  {
    var key = FormattableString.Invariant($"reverse?format=jsonv2&addressdetails=1&lat={lat}&lon={lng}&accept-language=pt-BR");
    if (cache.TryGetValue(key, out CreateAddressDto? cached) && cached is not null) return cached;
    await _gate.WaitAsync();
    try
    {
      if (cache.TryGetValue(key, out cached) && cached is not null) return cached;
      using var response = await http.GetAsync(key);
      response.EnsureSuccessStatusCode();
      using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
      var address = json.RootElement.TryGetProperty("address", out var value) ? value : default;
      string Get(params string[] names)
      {
        foreach (var name in names)
          if (address.ValueKind == JsonValueKind.Object && address.TryGetProperty(name, out var field) && field.ValueKind == JsonValueKind.String)
            return field.GetString() ?? "";
        return "";
      }
      var result = new CreateAddressDto(Get("road", "pedestrian", "residential"), Get("house_number"),
          Get("unit"), Get("suburb", "neighbourhood", "quarter"), Get("city", "town", "village", "municipality"),
          Get("state"), Get("postcode"), lat, lng);
      cache.Set(key, result, TimeSpan.FromHours(24));
      return result;
    }
    catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException)
    {
      logger.LogWarning(error, "Não foi possível consultar o endereço no Nominatim.");
      return new CreateAddressDto("", "", "", "", "", "", "", lat, lng);
    }
    finally
    {
      await Task.Delay(TimeSpan.FromSeconds(1));
      _gate.Release();
    }
  }
}
