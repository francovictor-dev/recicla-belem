namespace Backend.Dtos;

public record CreateAddressDto(string Road, string Number, string Complement, string Neighborhood, string City, string State, string ZipCode, double Lat, double Lng);
public record UpdateAddressDto(string Road, string Number, string Complement, string Neighborhood, string City, string State, string ZipCode, double Lat, double Lng);
public record AddressResponseDto(Guid Id, string Road, string Number, string Complement, string Neighborhood, string City, string State, string ZipCode, double Lat, double Lng, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);