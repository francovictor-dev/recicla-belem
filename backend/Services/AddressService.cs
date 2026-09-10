using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AddressService : IAddressService
{
    private readonly AppDbContext _context;

    public AddressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AddressResponseDto> CreateAsync(CreateAddressDto dto)
    {
        var address = new Address
        {
            Id = Guid.NewGuid(),
            Road = dto.Road,
            Number = dto.Number,
            Complement = dto.Complement,
            Neighborhood = dto.Neighborhood,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Lat = dto.Lat,
            Lng = dto.Lng,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
 
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return MapToDto(address);
    }

    public async Task<AddressResponseDto?> GetByIdAsync(Guid id)
    {
        var address = await _context.Addresses.FindAsync(id);
        return address is null ? null : MapToDto(address);
    }

    public async Task<IEnumerable<AddressResponseDto>> GetAllAsync()
    {
      var address = await _context.Addresses.ToListAsync();
      return address.Select(MapToDto);
    }

    public async Task<AddressResponseDto> UpdateAsync(Guid id, UpdateAddressDto dto)
    {
        var address = await _context.Addresses.FindAsync(id)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");

        address.Road = dto.Road;
        address.Number = dto.Number;
        address.Complement = dto.Complement;
        address.Neighborhood = dto.Neighborhood;
        address.City = dto.City;
        address.State = dto.State;
        address.ZipCode = dto.ZipCode;
        address.Lat = dto.Lat;
        address.Lng = dto.Lng;
        address.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(address);
    }

    public async Task DeleteAsync(Guid id)
    {
        var address = await _context.Addresses.FindAsync(id)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }

    private static AddressResponseDto MapToDto(Address address) =>
        new (
            address.Id,
            address.Road,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.State,
            address.ZipCode,
            address.Lat,
            address.Lng,
            address.CreatedAt,
            address.UpdatedAt
        );
}