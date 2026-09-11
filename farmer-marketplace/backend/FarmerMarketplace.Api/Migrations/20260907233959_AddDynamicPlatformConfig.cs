using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicPlatformConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequiresRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CanPerform = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PlatformConfigs",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "IsActive", "Key", "MaxValue", "MinValue", "RequiresRole", "UpdatedAt", "UpdatedBy", "Value", "ValueType" },
                values: new object[,]
                {
                    { 1, "pricing", new DateTime(2026, 9, 7, 23, 39, 58, 191, DateTimeKind.Utc).AddTicks(8562), "Minimum farmer can set per kg", true, "farmer_price_minimum_per_kg", 100m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 191, DateTimeKind.Utc).AddTicks(8543), null, "5.0", 0 },
                    { 2, "pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2626), "Maximum farmer can set per kg", true, "farmer_price_maximum_per_kg", 1000m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2616), null, "500.0", 0 },
                    { 3, "fees", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2684), "Platform commission on farmer price (8%)", true, "commission_pct", 1m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2669), null, "0.08", 2 },
                    { 4, "fees", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2711), "Payment gateway fee as % of buyer total (2%)", true, "payment_gateway_pct", 1m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2697), null, "0.02", 2 },
                    { 5, "fees", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2739), "Annual farmer subscription fee in rupees", true, "subscription_fee_per_farmer_per_year", 10000m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2725), null, "500.0", 0 },
                    { 6, "logistics", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2767), "Cost paid to logistics partner per kg (₹)", true, "logistics_partner_payout_per_kg", 50m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2753), null, "2.0", 0 },
                    { 7, "logistics", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2792), "Platform margin on logistics per kg (₹)", true, "logistics_platform_margin_per_kg", 50m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2782), null, "0.5", 0 },
                    { 8, "logistics", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2820), "Minimum flat delivery charge regardless of weight", true, "min_delivery_charge", 500m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2806), null, "20.0", 0 },
                    { 9, "logistics", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2844), "Max kg capacity per truck/vehicle", true, "max_weight_per_vehicle_kg", 10000m, 100m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2835), null, "2000.0", 0 },
                    { 10, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2873), "Auto-flag dispute if weight loss exceeds this %", true, "weight_loss_threshold_pct", 100m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2859), null, "5.0", 2 },
                    { 11, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2908), "Auto-refund without dispute if loss below this %", true, "weight_loss_auto_refund_pct", 100m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2898), null, "2.0", 2 },
                    { 12, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2936), "Hours buyer has to dispute after delivery", true, "dispute_resolution_window_hours", 168m, 1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2922), null, "24", 1 },
                    { 13, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2965), "Auto-release farmer payment after this many hours if no dispute", true, "auto_release_escrow_after_hours", 168m, 1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2951), null, "24", 1 },
                    { 14, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2989), "Minimum order size in kg", true, "min_order_quantity_kg", 1000m, 0.1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2979), null, "1.0", 0 },
                    { 15, "thresholds", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3059), "Maximum order size in kg", true, "max_order_quantity_kg", 100000m, 1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3045), null, "10000.0", 0 },
                    { 16, "payouts", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3082), "Run payout batch every N hours", true, "payout_batch_frequency_hours", 168m, 1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3073), null, "24", 1 },
                    { 17, "payouts", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3110), "Minimum accumulated amount to trigger payout (₹)", true, "minimum_payout_amount_rs", 10000m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3096), null, "100.0", 0 },
                    { 18, "payouts", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3139), "Preferred day for weekly payouts (0=Sunday, 1=Monday, etc)", true, "payout_day_of_week", 6m, 0m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3125), null, "1", 1 },
                    { 19, "costs", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3163), "Annual platform fixed costs (cloud/ops/marketing)", true, "annual_fixed_costs_rs", 10000000m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3149), null, "90000.0", 0 },
                    { 20, "costs", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3200), "Target net margin % on platform revenue", true, "target_net_margin_pct", 100m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3186), null, "20.0", 2 },
                    { 21, "crop_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3225), "Override commission % for tomatoes", true, "tomato_commission_pct", 1m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3215), null, "0.08", 2 },
                    { 22, "crop_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3253), "Override commission % for onions", true, "onion_commission_pct", 1m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3239), null, "0.08", 2 },
                    { 23, "crop_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3278), "Override commission % for potatoes", true, "potato_commission_pct", 1m, 0m, "superadmin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3268), null, "0.08", 2 },
                    { 24, "seasonal_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3306), "Price multiplier for Rabi Glut season", true, "s1_rabi_glut_multiplier", 2.0m, 0.1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3292), null, "1.0", 0 },
                    { 25, "seasonal_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3334), "Price multiplier for Pre-Monsoon season", true, "s2_pre_monsoon_multiplier", 2.0m, 0.1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3320), null, "1.15", 0 },
                    { 26, "seasonal_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3357), "Price multiplier for Monsoon season", true, "s3_monsoon_multiplier", 2.0m, 0.1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3344), null, "1.10", 0 },
                    { 27, "seasonal_pricing", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3385), "Price multiplier for Kharif season", true, "s4_kharif_multiplier", 2.0m, 0.1m, "admin", new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3371), null, "1.10", 0 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Action", "CanPerform", "Description", "Resource", "Role" },
                values: new object[,]
                {
                    { 1, "view_config", true, "Superadmin can view all config", "all", "superadmin" },
                    { 2, "edit_config", true, "Superadmin can edit all config", "all", "superadmin" },
                    { 3, "edit_pricing", true, "Superadmin can edit pricing", "all", "superadmin" },
                    { 4, "edit_critical", true, "Superadmin can edit critical business rules", "all", "superadmin" },
                    { 5, "view_config", true, "Admin can view all config", "all", "admin" },
                    { 6, "edit_config", true, "Admin can edit logistics", "logistics", "admin" },
                    { 7, "edit_config", true, "Admin can edit business thresholds", "thresholds", "admin" },
                    { 8, "edit_pricing", true, "Admin can edit logistics pricing", "logistics", "admin" },
                    { 9, "edit_critical", false, "Admin CANNOT edit critical business rules", "all", "admin" },
                    { 10, "view_config", true, "Manager can view all config", "all", "manager" },
                    { 11, "edit_config", false, "Manager cannot edit any config", "all", "manager" },
                    { 12, "edit_critical", false, "Manager cannot edit critical config", "all", "manager" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformConfigs");

            migrationBuilder.DropTable(
                name: "RolePermissions");
        }
    }
}
