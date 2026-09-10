using Backend.Data;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using backend.Enums;

namespace Backend.Services;

public class UserService(AppDbContext context) : IUserService
{
  private readonly AppDbContext _context = context;

  public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
  {
    var existEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
    if (existEmail)
      throw new InvalidOperationException("E-mail já cadastrado.");
    
    var user = new User
    {
      Id = Guid.NewGuid(),
      Email = dto.Email,
      Password = HashPassword(dto.Password),
      UserType = dto.UserType,
      UserRole = UserRole.Admin,
      IsActived = true,
      CreatedAt = DateTimeOffset.UtcNow
    };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return MapToDto(user);
  }
  public async Task<UserResponseDto?> GetByIdAsync(Guid id)
  {
    var user = await _context.Users.FindAsync(id);
    if (user is null)
      return null;
      
    return MapToDto(user);
  }
  public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
  {
    var users = await _context.Users.ToListAsync();
    return users.Select(MapToDto);
  }
  public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto)
  {
    var user = await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("Usuário não encontrado.");
    user.Email = dto.Email;
    user.UserType = dto.UserType;
    user.IsActived = dto.IsActived;
    user.UpdatedAt = DateTimeOffset.UtcNow;
    await _context.SaveChangesAsync();
    return MapToDto(user);
  }
  public async Task DeleteAsync(Guid id)
  {
    var user = await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("Usuário não encontrado.");
    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
  }
  private static string HashPassword(string password)
      => BCrypt.Net.BCrypt.HashPassword(password);
  private static UserResponseDto MapToDto(User user)
      => new(user.Id, user.Email, user.IsActived, user.UserType, user.UserRole, user.CreatedAt, user.UpdatedAt);
}