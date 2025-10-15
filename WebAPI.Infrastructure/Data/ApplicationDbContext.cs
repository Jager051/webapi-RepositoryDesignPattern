using Microsoft.EntityFrameworkCore;
using WebAPI.Core.Entities;

namespace WebAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ExceptionLog> ExceptionLogs { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();

                // Foreign key relationship
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ExceptionLog configuration
            modelBuilder.Entity<ExceptionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ExceptionType).HasMaxLength(200);
                entity.Property(e => e.Source).HasMaxLength(500);
                entity.Property(e => e.InnerException).HasMaxLength(2000);
                entity.Property(e => e.RequestPath).HasMaxLength(500);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Severity).HasMaxLength(20).HasDefaultValue("Error");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ExceptionLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes for better query performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Severity);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityName).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.RequestPath).HasMaxLength(500);
                entity.Property(e => e.HttpMethod).HasMaxLength(10);
                entity.Property(e => e.LogLevel).HasMaxLength(20).HasDefaultValue("Information");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes for better query performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => new { e.EntityName, e.EntityId });
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Clothing", Description = "Fashion and apparel", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Books", Description = "Books and literature", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, SKU = "LAP001", StockQuantity = 10, CategoryId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Product { Id = 2, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, SKU = "TSH001", StockQuantity = 50, CategoryId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Product { Id = 3, Name = "Programming Book", Description = "Learn C# programming", Price = 49.99m, SKU = "BK001", StockQuantity = 25, CategoryId = 3, IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), FirstName = "Admin", LastName = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Id = 2, Username = "user1", Email = "user@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"), FirstName = "John", LastName = "Doe", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }
    }
}
