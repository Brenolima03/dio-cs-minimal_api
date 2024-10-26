using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;

namespace minimal_api.Infra.Db;

public class Db (
  DbContextOptions<Db> options, IConfiguration configAppSettings
) : DbContext(options)
{
  private readonly IConfiguration _configAppSettings = configAppSettings;

  public DbSet<Admin> Admins { get; set; } = default!;
  public DbSet<Vehicle> Vehicles { get; set; } = default!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Admin>().HasData(
      new Admin {
        Id = 1,
        Email = "admin@admin.com",
        Password = "123456",
        Profile = "Adm"
      }
    );
  }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var connectionString = _configAppSettings.GetConnectionString("Mysql");
      if (!string.IsNullOrEmpty(connectionString))
      {
        optionsBuilder.UseMySql(
          connectionString, ServerVersion.AutoDetect(connectionString)
        );
      }
    }
  }
}
