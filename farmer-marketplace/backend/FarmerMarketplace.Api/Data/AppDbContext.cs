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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<TokenBlocklist> TokenBlocklist { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentSplit> PaymentSplits { get; set; }
        public DbSet<FarmerMarketplace.Api.Models.Route> Routes { get; set; }
        public DbSet<FarmerMarketplace.Api.Models.RouteStop> RouteStops { get; set; }
        public DbSet<DeliveryRoute> DeliveryRoutes { get; set; }
        public DbSet<BatchRunLog> BatchRunLogs { get; set; }

        // --- Payment System & Financial Model DbSets ---
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; }
        public DbSet<PlatformFee> PlatformFees { get; set; }
        public DbSet<FarmerPayout> FarmerPayouts { get; set; }
        public DbSet<TransactionLedger> TransactionLedgers { get; set; }
        public DbSet<DeliveryWeightLog> DeliveryWeightLogs { get; set; }

        // --- Dynamic Platform Configuration & RBAC DbSets ---
        public DbSet<PlatformConfig> PlatformConfigs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // --- AI Demand Forecasting DbSet ---
        public DbSet<SalesHistory> SalesHistories { get; set; }

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
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentSplit>()
                .HasOne(s => s.Payment)
                .WithMany(p => p.Splits)
                .HasForeignKey(s => s.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentSplit>()
                .HasOne(s => s.Farmer)
                .WithMany()
                .HasForeignKey(s => s.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Stops)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RouteStop>()
                .HasOne(s => s.Order)
                .WithMany()
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Escrow & Financial System Relationships
            modelBuilder.Entity<EscrowTransaction>()
                .HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EscrowTransaction>()
                .HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlatformFee>()
                .HasOne(pf => pf.Escrow)
                .WithMany()
                .HasForeignKey(pf => pf.EscrowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlatformFee>()
                .HasOne(pf => pf.Order)
                .WithMany()
                .HasForeignKey(pf => pf.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmerPayout>()
                .HasOne(fp => fp.Farmer)
                .WithMany()
                .HasForeignKey(fp => fp.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmerPayout>()
                .HasOne(fp => fp.FpoAdmin)
                .WithMany()
                .HasForeignKey(fp => fp.FpoAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionLedger>()
                .HasOne(tl => tl.Order)
                .WithMany()
                .HasForeignKey(tl => tl.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliveryWeightLog>()
                .HasOne(dw => dw.Route)
                .WithMany()
                .HasForeignKey(dw => dw.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed SuperAdmin User
            var superAdminId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = superAdminId,
                    Name = "superadmin",
                    Email = "superadmin@fasalconnect.com",
                    Phone = "9999999999",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = UserRole.SuperAdmin,
                    Location = "New Delhi",
                    PreferredLanguage = "en",
                    IsProfileComplete = true,
                    Suspended = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Role Permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 1, Role = "superadmin", Action = "view_config", Resource = "all", CanPerform = true, Description = "Superadmin can view all config" },
                new RolePermission { Id = 2, Role = "superadmin", Action = "edit_config", Resource = "all", CanPerform = true, Description = "Superadmin can edit all config" },
                new RolePermission { Id = 3, Role = "superadmin", Action = "edit_pricing", Resource = "all", CanPerform = true, Description = "Superadmin can edit pricing" },
                new RolePermission { Id = 4, Role = "superadmin", Action = "edit_critical", Resource = "all", CanPerform = true, Description = "Superadmin can edit critical business rules" },
                new RolePermission { Id = 5, Role = "admin", Action = "view_config", Resource = "all", CanPerform = true, Description = "Admin can view all config" },
                new RolePermission { Id = 6, Role = "admin", Action = "edit_config", Resource = "logistics", CanPerform = true, Description = "Admin can edit logistics" },
                new RolePermission { Id = 7, Role = "admin", Action = "edit_config", Resource = "thresholds", CanPerform = true, Description = "Admin can edit business thresholds" },
                new RolePermission { Id = 8, Role = "admin", Action = "edit_pricing", Resource = "logistics", CanPerform = true, Description = "Admin can edit logistics pricing" },
                new RolePermission { Id = 9, Role = "admin", Action = "edit_critical", Resource = "all", CanPerform = false, Description = "Admin CANNOT edit critical business rules" },
                new RolePermission { Id = 10, Role = "manager", Action = "view_config", Resource = "all", CanPerform = true, Description = "Manager can view all config" },
                new RolePermission { Id = 11, Role = "manager", Action = "edit_config", Resource = "all", CanPerform = false, Description = "Manager cannot edit any config" },
                new RolePermission { Id = 12, Role = "manager", Action = "edit_critical", Resource = "all", CanPerform = false, Description = "Manager cannot edit critical config" }
            );

            // Seed Platform Config
            modelBuilder.Entity<PlatformConfig>().HasData(
                new PlatformConfig { Id = 1, Category = "pricing", Key = "farmer_price_minimum_per_kg", Value = "5.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 100m, RequiresRole = "superadmin", Description = "Minimum farmer can set per kg" },
                new PlatformConfig { Id = 2, Category = "pricing", Key = "farmer_price_maximum_per_kg", Value = "500.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 1000m, RequiresRole = "superadmin", Description = "Maximum farmer can set per kg" },
                new PlatformConfig { Id = 3, Category = "fees", Key = "commission_pct", Value = "0.08", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 1m, RequiresRole = "superadmin", Description = "Platform commission on farmer price (8%)" },
                new PlatformConfig { Id = 4, Category = "fees", Key = "payment_gateway_pct", Value = "0.02", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 1m, RequiresRole = "superadmin", Description = "Payment gateway fee as % of buyer total (2%)" },
                new PlatformConfig { Id = 5, Category = "fees", Key = "subscription_fee_per_farmer_per_year", Value = "500.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 10000m, RequiresRole = "superadmin", Description = "Annual farmer subscription fee in rupees" },
                new PlatformConfig { Id = 6, Category = "logistics", Key = "logistics_partner_payout_per_kg", Value = "2.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 50m, RequiresRole = "admin", Description = "Cost paid to logistics partner per kg (₹)" },
                new PlatformConfig { Id = 7, Category = "logistics", Key = "logistics_platform_margin_per_kg", Value = "0.5", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 50m, RequiresRole = "admin", Description = "Platform margin on logistics per kg (₹)" },
                new PlatformConfig { Id = 8, Category = "logistics", Key = "min_delivery_charge", Value = "20.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 500m, RequiresRole = "admin", Description = "Minimum flat delivery charge regardless of weight" },
                new PlatformConfig { Id = 9, Category = "logistics", Key = "max_weight_per_vehicle_kg", Value = "2000.0", ValueType = ConfigValueType.Decimal, MinValue = 100m, MaxValue = 10000m, RequiresRole = "admin", Description = "Max kg capacity per truck/vehicle" },
                new PlatformConfig { Id = 10, Category = "thresholds", Key = "weight_loss_threshold_pct", Value = "5.0", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 100m, RequiresRole = "admin", Description = "Auto-flag dispute if weight loss exceeds this %" },
                new PlatformConfig { Id = 11, Category = "thresholds", Key = "weight_loss_auto_refund_pct", Value = "2.0", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 100m, RequiresRole = "admin", Description = "Auto-refund without dispute if loss below this %" },
                new PlatformConfig { Id = 12, Category = "thresholds", Key = "dispute_resolution_window_hours", Value = "24", ValueType = ConfigValueType.Integer, MinValue = 1m, MaxValue = 168m, RequiresRole = "admin", Description = "Hours buyer has to dispute after delivery" },
                new PlatformConfig { Id = 13, Category = "thresholds", Key = "auto_release_escrow_after_hours", Value = "24", ValueType = ConfigValueType.Integer, MinValue = 1m, MaxValue = 168m, RequiresRole = "admin", Description = "Auto-release farmer payment after this many hours if no dispute" },
                new PlatformConfig { Id = 14, Category = "thresholds", Key = "min_order_quantity_kg", Value = "10.0", ValueType = ConfigValueType.Decimal, MinValue = 0.1m, MaxValue = 1000m, RequiresRole = "admin", Description = "Minimum order size in kg" },
                new PlatformConfig { Id = 15, Category = "thresholds", Key = "max_order_quantity_kg", Value = "10000.0", ValueType = ConfigValueType.Decimal, MinValue = 1m, MaxValue = 100000m, RequiresRole = "admin", Description = "Maximum order size in kg" },
                new PlatformConfig { Id = 16, Category = "payouts", Key = "payout_batch_frequency_hours", Value = "24", ValueType = ConfigValueType.Integer, MinValue = 1m, MaxValue = 168m, RequiresRole = "admin", Description = "Run payout batch every N hours" },
                new PlatformConfig { Id = 17, Category = "payouts", Key = "minimum_payout_amount_rs", Value = "100.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 10000m, RequiresRole = "admin", Description = "Minimum accumulated amount to trigger payout (₹)" },
                new PlatformConfig { Id = 18, Category = "payouts", Key = "payout_day_of_week", Value = "1", ValueType = ConfigValueType.Integer, MinValue = 0m, MaxValue = 6m, RequiresRole = "admin", Description = "Preferred day for weekly payouts (0=Sunday, 1=Monday, etc)" },
                new PlatformConfig { Id = 19, Category = "costs", Key = "annual_fixed_costs_rs", Value = "90000.0", ValueType = ConfigValueType.Decimal, MinValue = 0m, MaxValue = 10000000m, RequiresRole = "superadmin", Description = "Annual platform fixed costs (cloud/ops/marketing)" },
                new PlatformConfig { Id = 20, Category = "costs", Key = "target_net_margin_pct", Value = "20.0", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 100m, RequiresRole = "superadmin", Description = "Target net margin % on platform revenue" },
                new PlatformConfig { Id = 21, Category = "crop_pricing", Key = "tomato_commission_pct", Value = "0.08", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 1m, RequiresRole = "superadmin", Description = "Override commission % for tomatoes" },
                new PlatformConfig { Id = 22, Category = "crop_pricing", Key = "onion_commission_pct", Value = "0.08", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 1m, RequiresRole = "superadmin", Description = "Override commission % for onions" },
                new PlatformConfig { Id = 23, Category = "crop_pricing", Key = "potato_commission_pct", Value = "0.08", ValueType = ConfigValueType.Percent, MinValue = 0m, MaxValue = 1m, RequiresRole = "superadmin", Description = "Override commission % for potatoes" },
                new PlatformConfig { Id = 24, Category = "seasonal_pricing", Key = "s1_rabi_glut_multiplier", Value = "1.0", ValueType = ConfigValueType.Decimal, MinValue = 0.1m, MaxValue = 2.0m, RequiresRole = "admin", Description = "Price multiplier for Rabi Glut season" },
                new PlatformConfig { Id = 25, Category = "seasonal_pricing", Key = "s2_pre_monsoon_multiplier", Value = "1.15", ValueType = ConfigValueType.Decimal, MinValue = 0.1m, MaxValue = 2.0m, RequiresRole = "admin", Description = "Price multiplier for Pre-Monsoon season" },
                new PlatformConfig { Id = 26, Category = "seasonal_pricing", Key = "s3_monsoon_multiplier", Value = "1.10", ValueType = ConfigValueType.Decimal, MinValue = 0.1m, MaxValue = 2.0m, RequiresRole = "admin", Description = "Price multiplier for Monsoon season" },
                new PlatformConfig { Id = 27, Category = "seasonal_pricing", Key = "s4_kharif_multiplier", Value = "1.10", ValueType = ConfigValueType.Decimal, MinValue = 0.1m, MaxValue = 2.0m, RequiresRole = "admin", Description = "Price multiplier for Kharif season" }
            );
        }
    }
}
