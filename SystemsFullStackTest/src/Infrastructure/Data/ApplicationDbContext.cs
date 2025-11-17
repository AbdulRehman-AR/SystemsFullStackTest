using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Parent> Parents { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Canteen> Canteens { get; set; }
    public DbSet<CanteenSchedule> CanteenSchedules { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.WalletBalance).HasPrecision(18, 2);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Parent)
                .WithMany(p => p.Students)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Canteen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<CanteenSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Canteen)
                .WithMany(c => c.Schedules)
                .HasForeignKey(e => e.CanteenId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CanteenId, e.DayOfWeek }).IsUnique();
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Canteen)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(e => e.CanteenId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.AllergenTags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<ICollection<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.Parent)
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Canteen)
                .WithMany()
                .HasForeignKey(e => e.CanteenId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MenuItem)
                .WithMany()
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

