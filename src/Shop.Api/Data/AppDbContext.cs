using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shop.Api.Data.Entities;

namespace Shop.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Precision configurations
        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Brand).IsRequired().HasMaxLength(100);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(o => o.OrderNumber).IsUnique();

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
