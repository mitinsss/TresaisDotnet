using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Models;

namespace PraktiskaisDarbs3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasMany(i => i.OrderItems)
                .WithOne(oi => oi.Item)
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Item>().Property(i => i.Name).HasMaxLength(150).IsRequired();
            modelBuilder.Entity<Order>().Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
