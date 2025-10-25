using EmployeeStatus.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeStatus.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Salary> Salaries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NationalNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsActive).IsRequired();
            
            entity.HasIndex(e => e.NationalNumber).IsUnique();
        });

        // Configure Salary entity
        modelBuilder.Entity<Salary>(entity =>
        {
            entity.ToTable("Salaries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.SalaryAmount).IsRequired().HasColumnType("numeric(18,2)").HasColumnName("Salary");
            entity.Property(e => e.UserId).IsRequired();
            
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.UserId);
        });
    }
}
