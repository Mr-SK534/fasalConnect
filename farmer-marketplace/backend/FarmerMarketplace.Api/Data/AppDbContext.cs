// backend/FarmerMarketplace.Api/Data/AppDbContext.cs

using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Fpo)
                .WithMany()
                .HasForeignKey(u => u.FpoId)
                .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // deleting an order deletes its items

                modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // don't delete order history if product is deleted

                modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Farmer)
                .WithMany()
                .HasForeignKey(oi => oi.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

                 modelBuilder.Entity<Order>()
                 .HasOne(o => o.Buyer)
                 .WithMany()
                 .HasForeignKey(o => o.BuyerId)
                 .OnDelete(DeleteBehavior.Restrict); 
        }   

        public DbSet<Product> Products { get; set; }    
        // backend/FarmerMarketplace.Api/Data/AppDbContext.cs (add this DbSet)

        public DbSet<TokenBlocklist> TokenBlocklist { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

                                        
    }
}