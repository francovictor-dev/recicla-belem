// Backend/Models/RefreshToken.cs
namespace Backend.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;
}