using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ExternalLoginService(AppDbContext context) : IExternalLoginService
{
  private readonly AppDbContext _context = context;

  public async Task<ExternalLoginResponseDto> CreateAsync(CreateExternalLoginDto dto)
  {
    await ValidateUserAsync(dto.UserId);

    var now = DateTimeOffset.UtcNow;
    var externalLogin = new ExternalLogin
    {
      Id = Guid.NewGuid(),
      Provider = dto.Provider,
      ProviderId = dto.ProviderId,
      UserId = dto.UserId,
      CreatedAt = now,
      UpdatedAt = now
    };

    _context.ExternalLogins.Add(externalLogin);
    await _context.SaveChangesAsync();
    return MapToDto(externalLogin);
  }

  public async Task<ExternalLoginResponseDto?> GetByIdAsync(Guid id)
  {
    var externalLogin = await _context.ExternalLogins.FindAsync(id);
    return externalLogin is null ? null : MapToDto(externalLogin);
  }

  public async Task<IEnumerable<ExternalLoginResponseDto>> GetAllAsync()
  {
    var externalLogins = await _context.ExternalLogins.ToListAsync();
    return externalLogins.Select(MapToDto);
  }

  public async Task<ExternalLoginResponseDto> UpdateAsync(Guid id, UpdateExternalLoginDto dto)
  {
    var externalLogin = await _context.ExternalLogins.FindAsync(id)
        ?? throw new KeyNotFoundException("Login externo não encontrado.");

    await ValidateUserAsync(dto.UserId);

    externalLogin.Provider = dto.Provider;
    externalLogin.ProviderId = dto.ProviderId;
    externalLogin.UserId = dto.UserId;
    externalLogin.UpdatedAt = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();
    return MapToDto(externalLogin);
  }

  public async Task DeleteAsync(Guid id)
  {
    var externalLogin = await _context.ExternalLogins.FindAsync(id)
        ?? throw new KeyNotFoundException("Login externo não encontrado.");

    _context.ExternalLogins.Remove(externalLogin);
    await _context.SaveChangesAsync();
  }

  private async Task ValidateUserAsync(Guid userId)
  {
    if (!await _context.Users.AnyAsync(user => user.Id == userId))
      throw new KeyNotFoundException("Usuário não encontrado.");
  }

  private static ExternalLoginResponseDto MapToDto(ExternalLogin externalLogin) =>
      new(externalLogin.Id, externalLogin.Provider, externalLogin.ProviderId, externalLogin.UserId, externalLogin.CreatedAt, externalLogin.UpdatedAt);
}
