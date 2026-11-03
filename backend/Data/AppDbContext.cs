using Backend.Models;
using Microsoft.EntityFrameworkCore;
namespace Backend.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {

  }

  public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Profile> Profiles => Set<Profile>();
  public DbSet<Address> Addresses => Set<Address>();
  public DbSet<Collector> Collectors => Set<Collector>();
  public DbSet<CollectionPoint> CollectionPoints => Set<CollectionPoint>();
  public DbSet<CollectionPointCollector> CollectionPointCollectors => Set<CollectionPointCollector>();
  public DbSet<Image> Images => Set<Image>();
  public DbSet<Report> Reports => Set<Report>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ExternalLogin>()
        .ToTable("ExternalLogin");

    modelBuilder.Entity<ExternalLogin>()
        .HasOne(externalLogin => externalLogin.User)
        .WithMany()
        .HasForeignKey(externalLogin => externalLogin.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RefreshToken>()
        .HasOne(rt => rt.User)
        .WithMany(u => u.RefreshTokens)
        .HasForeignKey(rt => rt.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RefreshToken>()
        .HasIndex(rt => rt.Token)
        .IsUnique(); // ✅ índice único que faltou

    modelBuilder.Entity<Image>()
      .HasOne(image => image.CollectionPoint)
      .WithMany(collectionPoint => collectionPoint.Images)
      .HasForeignKey(image => image.CollectionPointId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Report>()
      .HasOne(report => report.User)
      .WithMany()
      .HasForeignKey(report => report.UserId)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Report>()
      .HasOne(report => report.CollectionPoint)
      .WithMany()
      .HasForeignKey(report => report.CollectionPointId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}