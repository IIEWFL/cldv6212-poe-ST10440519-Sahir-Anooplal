using Microsoft.EntityFrameworkCore;
using ABCRetailPart3.Models;

namespace ABCRetailPart3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.Password).IsRequired();
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(256);
                entity.Property(o => o.Status).IsRequired().HasMaxLength(50);
                entity.Property(o => o.ShippingAddress).IsRequired();

                // Relationship: User has many Orders
                entity.HasOne(o => o.Customer)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                // Relationship: User has many CartItems
                entity.HasOne(ci => ci.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}