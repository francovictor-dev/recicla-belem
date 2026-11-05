using Backend.Data;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using backend.Enums;

namespace Backend.Services;

public class UserService(AppDbContext context, IS3Service s3Service) : IUserService
{
  private readonly AppDbContext _context = context;

  public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    => await CreateUserAsync(dto, UserRole.Admin);

  public async Task<UserResponseDto> CreateDefaultAsync(CreateUserDto dto, CreateProfileDto profileDto, DefaultExternalLoginDto? externalLoginDto = null, IFormFile? image = null)
  {
    if (externalLoginDto is not null &&
        (!Enum.IsDefined(externalLoginDto.Provider) || string.IsNullOrWhiteSpace(externalLoginDto.ProviderId)))
      throw new ArgumentException("Informe um provedor válido e seu identificador.");

    byte[]? imageBytes = null;
    if (image is not null)
    {
      if (image.Length <= 0 || image.Length > 300 * 1024 || image.ContentType != "image/png")
        throw new ArgumentException("A imagem deve ser PNG e ter até 300 KB.");
      await using var input = image.OpenReadStream();
      using var buffer = new MemoryStream();
      await input.CopyToAsync(buffer);
      imageBytes = buffer.ToArray();
      byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
      if (imageBytes.Length > 300 * 1024 || !imageBytes.AsSpan().StartsWith(signature))
        throw new ArgumentException("O arquivo deve ser uma imagem PNG.");
    }

    var strategy = _context.Database.CreateExecutionStrategy();
    var firstAttempt = true;
    return await strategy.ExecuteAsync(async () =>
    {
      if (!firstAttempt) _context.ChangeTracker.Clear();
      firstAttempt = false;
      await using var transaction = await _context.Database.BeginTransactionAsync();
      string? uploadedKey = null;
      try
      {
        var user = await CreateUserAsync(dto, UserRole.Default);
        var profile = new Profile
        {
          Id = Guid.NewGuid(),
          Name = profileDto.Name,
          PhotoUrl = profileDto.PhotoUrl,
          UserId = user.Id
        };
        _context.Profiles.Add(profile);

        if (externalLoginDto is not null)
        {
          var now = DateTimeOffset.UtcNow;
          _context.ExternalLogins.Add(new ExternalLogin
          {
            Id = Guid.NewGuid(),
            Provider = externalLoginDto.Provider,
            ProviderId = externalLoginDto.ProviderId,
            UserId = user.Id,
            CreatedAt = now,
            UpdatedAt = now
          });
        }
        await _context.SaveChangesAsync();

        if (imageBytes is not null)
        {
          var key = $"users/{user.Id}/profile.png";
          using var content = new MemoryStream(imageBytes);
          profile.PhotoUrl = await s3Service.UploadAsync(content, key, "image/png");
          uploadedKey = key;
          await _context.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return user;
      }
      catch (Exception error)
      {
        if (uploadedKey is not null)
        {
          try
          {
            await s3Service.DeleteAsync(uploadedKey);
          }
          catch (Exception cleanupError)
          {
            throw new AggregateException("Falha no cadastro e na remoção da imagem do S3.", error, cleanupError);
          }
        }
        throw;
      }
    });
  }

  private async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, UserRole role)
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
      UserRole = role,
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
