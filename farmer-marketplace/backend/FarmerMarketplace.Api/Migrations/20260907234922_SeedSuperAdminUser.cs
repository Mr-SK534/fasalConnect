using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuperAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryWeightLogs_Orders_OrderId",
                table: "DeliveryWeightLogs");

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(536), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(522) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4439), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4430) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4468), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4454) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4492), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4482) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4515), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4506) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4543), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4529) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4571), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4562) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4624), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4614) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4648), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4638) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4720), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4705) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4744), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4730) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4768), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4759) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4793), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4783) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4821), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4807) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4886), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4872) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4929), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4914) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4952), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4943) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4976), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4967) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5004), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(4990) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5028), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5018) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5052), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5042) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5076), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5066) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5105), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5090) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5129), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5114) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5152), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5143) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5180), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5166) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5203), new DateTime(2026, 9, 7, 23, 49, 21, 358, DateTimeKind.Utc).AddTicks(5189) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountHolderName", "Address", "BankAccountNumber", "BankIfsc", "BusinessName", "CreatedAt", "DeliveryAddress", "District", "Email", "FpoId", "GstNumber", "IsProfileComplete", "Latitude", "Location", "Longitude", "Name", "PasswordHash", "Phone", "Pincode", "PreferredLanguage", "PrimaryCrops", "Region", "Role", "State", "Suspended", "SuspensionReason", "UpdatedAt", "UpiId" },
                values: new object[] { new Guid("a0000000-0000-0000-0000-000000000001"), null, null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "superadmin@fasalconnect.com", null, null, true, null, "New Delhi", null, "superadmin", "$2a$11$Ju8edbIa8vH7FCYaPb97EePLsdfnQA.6Zs1G/1AfUuOS.Y8EZFswq", "9999999999", null, "en", null, null, 4, null, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryWeightLogs_Orders_OrderId",
                table: "DeliveryWeightLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryWeightLogs_Orders_OrderId",
                table: "DeliveryWeightLogs");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 191, DateTimeKind.Utc).AddTicks(8562), new DateTime(2026, 9, 7, 23, 39, 58, 191, DateTimeKind.Utc).AddTicks(8543) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2626), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2616) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2684), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2669) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2711), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2697) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2739), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2725) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2767), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2753) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2792), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2782) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2820), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2806) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2844), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2835) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2873), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2859) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2908), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2898) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2936), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2922) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2965), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2951) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2989), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(2979) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3059), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3045) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3082), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3073) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3110), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3096) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3139), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3125) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3163), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3149) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3200), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3186) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3225), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3215) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3253), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3239) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3278), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3268) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3306), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3292) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3334), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3320) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3357), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3344) });

            migrationBuilder.UpdateData(
                table: "PlatformConfigs",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3385), new DateTime(2026, 9, 7, 23, 39, 58, 192, DateTimeKind.Utc).AddTicks(3371) });

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryWeightLogs_Orders_OrderId",
                table: "DeliveryWeightLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
