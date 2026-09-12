using Microsoft.EntityFrameworkCore;
using ai_dev_asst_api.Models;

namespace ai_dev_asst_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite primary key for RolePermissions
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Unique constraint on Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique constraint on RoleName
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        // Unique constraint on PermissionName
        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.PermissionName)
            .IsUnique();

        // Seed permissions
        var permAccessUser = new Permission
        {
            PermissionId = Guid.Parse(PermissionIds.AccessUser),
            PermissionName = "Allow User to Access User"
        };

        var permCreateEditRoles = new Permission
        {
            PermissionId = Guid.Parse(PermissionIds.CreateEditRoles),
            PermissionName = "Allow User to Create & Edit Roles"
        };

        modelBuilder.Entity<Permission>().HasData(permAccessUser, permCreateEditRoles);

        // Seed Admin role
        var adminRole = new Role
        {
            RoleId = 1,
            RoleName = "Admin",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        };
        modelBuilder.Entity<Role>().HasData(adminRole);

        // Seed Admin role permissions (both permissions)
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = 1, PermissionId = permAccessUser.PermissionId },
            new RolePermission { RoleId = 1, PermissionId = permCreateEditRoles.PermissionId }
        );

        // Seed Admin user (password: saravanan)
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1,
            FirstName = "Elavarasan",
            LastName = "Saravanan",
            Phone = "7010315801",
            Email = "elavarasan261992@gmail.com",
            // Static hash for "saravanan" — do NOT use BCrypt.HashPassword() here as it generates a new hash each migration
            PasswordHash = "$2a$11$lXEwCzE9ZDByR1iehY1rVOnh.DTU3nomnebFmAUTJyyvtUIJYEkS2",
            RoleId = 1,
            Theme = "default",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = false
        });
    }
}
