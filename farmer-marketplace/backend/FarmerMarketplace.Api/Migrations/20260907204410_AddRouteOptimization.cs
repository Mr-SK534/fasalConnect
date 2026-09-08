using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryLat",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLng",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedArrival",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLat",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLng",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RouteId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StopSequence",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleNumber",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BatchWindow = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VehicleCount = table.Column<int>(type: "integer", nullable: false),
                    TotalDistanceKm = table.Column<double>(type: "double precision", nullable: false),
                    StopsJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRoutes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RouteId",
                table: "Orders",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryRoutes_RouteId",
                table: "Orders",
                column: "RouteId",
                principalTable: "DeliveryRoutes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryRoutes_RouteId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryRoutes");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RouteId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryLat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryLng",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "EstimatedArrival",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupLat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupLng",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StopSequence",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VehicleNumber",
                table: "Orders");
        }
    }
}
