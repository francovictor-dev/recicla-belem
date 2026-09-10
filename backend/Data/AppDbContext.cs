using Backend.Models;
using Microsoft.EntityFrameworkCore;
namespace Backend.Data;
public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){

  }

  public DbSet<User> Users => Set<User>();
  public DbSet<Profile> Profiles => Set<Profile>();
  public DbSet<Address> Addresses => Set<Address>();
  public DbSet<Collector> Collectors => Set<Collector>();
  public DbSet<CollectionPoint> CollectionPoints => Set<CollectionPoint>();
  public DbSet<CollectionPointCollector> CollectionPointCollectors => Set<CollectionPointCollector>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
      modelBuilder.Entity<RefreshToken>()
          .HasOne(rt => rt.User)
          .WithMany(u => u.RefreshTokens)
          .HasForeignKey(rt => rt.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<RefreshToken>()
          .HasIndex(rt => rt.Token)
          .IsUnique(); // ✅ índice único que faltou
  }
}