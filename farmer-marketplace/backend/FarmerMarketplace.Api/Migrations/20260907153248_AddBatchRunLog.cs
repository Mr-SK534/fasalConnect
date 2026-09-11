using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchRunLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchRunLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    OrdersRouted = table.Column<int>(type: "integer", nullable: false),
                    RoutesCreated = table.Column<int>(type: "integer", nullable: false),
                    SkippedOrders = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchRunLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchRunLogs");
        }
    }
}
