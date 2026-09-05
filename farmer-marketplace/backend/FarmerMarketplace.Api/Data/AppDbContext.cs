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
        }
    }
}