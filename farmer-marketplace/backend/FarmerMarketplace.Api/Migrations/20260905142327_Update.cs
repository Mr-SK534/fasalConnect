using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIfsc",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryCrops",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpiId",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CropName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    HarvestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FarmerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Region = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Users_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenBlocklist",
                columns: table => new
                {
                    Jti = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBlocklist", x => x.Jti);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_FarmerId",
                table: "Products",
                column: "FarmerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "TokenBlocklist");

            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BankIfsc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GstNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrimaryCrops",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpiId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Users");
        }
    }
}
