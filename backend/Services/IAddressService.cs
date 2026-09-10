using Backend.Dtos;
using Backend.Services;

public interface IAddressService : ICrudService<CreateAddressDto, UpdateAddressDto, AddressResponseDto>
{
}