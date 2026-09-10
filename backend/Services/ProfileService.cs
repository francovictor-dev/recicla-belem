// Backend/Services/ProfileService.cs
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileResponseDto> CreateAsync(Guid userId, CreateProfileDto dto)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new KeyNotFoundException("Usuário não encontrado.");

        var alreadyHasProfile = await _context.Profiles.AnyAsync(p => p.UserId == userId);
        if (alreadyHasProfile)
            throw new InvalidOperationException("Usuário já possui um perfil.");

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            UserId = userId
        };

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<ProfileResponseDto?> GetByIdAsync(Guid id)
    {
        var profile = await _context.Profiles.FindAsync(id);
        return profile is null ? null : MapToDto(profile);
    }

    public async Task<ProfileResponseDto> UpdateAsync(Guid id, UpdateProfileDto dto)
    {
        var profile = await _context.Profiles.FindAsync(id)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        profile.Name = dto.Name;

        await _context.SaveChangesAsync();
        return MapToDto(profile);
    }

    public async Task DeleteAsync(Guid id)
    {
        var profile = await _context.Profiles.FindAsync(id)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
    }

    private static ProfileResponseDto MapToDto(Profile profile)
        => new(profile.Id, profile.Name, profile.UserId);
}