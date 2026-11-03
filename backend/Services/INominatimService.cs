using Backend.Dtos;

namespace Backend.Services;

public interface INominatimService
{
  Task<CreateAddressDto> ReverseAsync(double lat, double lng);
}
