using System.Net.Mail;
using Backend.Enums;
using Backend.Models;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class AdminUserSeeder
{
  public static async Task SeedAsync(AppDbContext context, IConfiguration configuration, CancellationToken cancellationToken = default, ILogger? logger = null)
  {
    var email = configuration["USER_ADMIN_EMAIL"]?.Trim();
    var password = configuration["USER_ADMIN_PW"];
    if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
    {
      logger?.LogWarning("Seeder Admin ignorado: USER_ADMIN_EMAIL e USER_ADMIN_PW não estão configuradas no processo.");
      return;
    }
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
        !MailAddress.TryCreate(email, out var address) || address.Address != email)
      throw new InvalidOperationException("Configure USER_ADMIN_EMAIL e USER_ADMIN_PW com valores válidos.");

    var normalizedEmail = email.ToLowerInvariant();
    var existing = await context.Users.SingleOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    if (existing is not null)
    {
      var passwordMatches = false;
      try { passwordMatches = BCrypt.Net.BCrypt.Verify(password, existing.Password); }
      catch (BCrypt.Net.SaltParseException) { }
      if (existing.UserRole == UserRole.Admin && existing.UserType == UserType.Email && existing.IsActived && passwordMatches)
      {
        logger?.LogInformation("Seeder Admin: conta já configurada, sem alterações.");
        return;
      }
      existing.UserRole = UserRole.Admin;
      existing.UserType = UserType.Email;
      existing.IsActived = true;
      if (!passwordMatches) existing.Password = BCrypt.Net.BCrypt.HashPassword(password);
      existing.UpdatedAt = DateTimeOffset.UtcNow;
      await context.SaveChangesAsync(cancellationToken);
      logger?.LogInformation("Seeder Admin: conta existente atualizada.");
      return;
    }

    var now = DateTimeOffset.UtcNow;
    context.Users.Add(new User
    {
      Id = Guid.NewGuid(),
      Email = email,
      Password = BCrypt.Net.BCrypt.HashPassword(password),
      UserRole = UserRole.Admin,
      UserType = UserType.Email,
      IsActived = true,
      CreatedAt = now,
      UpdatedAt = now
    });
    await context.SaveChangesAsync(cancellationToken);
    logger?.LogInformation("Seeder Admin: conta criada.");
  }
}
