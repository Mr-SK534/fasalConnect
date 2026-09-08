using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmerMarketplace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEscrowSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CropName",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryConfirmedDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDateTarget",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FarmerAskingPricePerKg",
                table: "Orders",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FarmerId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FpoAdminId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityDeliveredKg",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityOrderedKg",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityPickedUpKg",
                table: "Orders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Season",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryWeightLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightAtPickupKg = table.Column<double>(type: "double precision", nullable: false),
                    DeliveryDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WeightAtDeliveryKg = table.Column<double>(type: "double precision", nullable: true),
                    WeightVarianceKg = table.Column<double>(type: "double precision", nullable: true),
                    VarianceReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryWeightLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryWeightLogs_DeliveryRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryWeightLogs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscrowTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrderedAmountRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ActualAmountRs = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HeldDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisputeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisputeResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarmerPayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FpoAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderIdsJson = table.Column<string>(type: "text", nullable: false),
                    TotalAmountRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    PayoutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    UpiIdOrBankAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConfirmationTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerPayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmerPayouts_Users_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarmerPayouts_Users_FpoAdminId",
                        column: x => x.FpoAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionLedgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionLedgers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscrowId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPct = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    CommissionAmountRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    LogisticsPartnerChargePerKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LogisticsPlatformMarginPerKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalLogisticsChargeRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    PaymentGatewayFeePct = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    PaymentGatewayFeeRs = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformFees_EscrowTransactions_EscrowId",
                        column: x => x.EscrowId,
                        principalTable: "EscrowTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformFees_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FarmerId",
                table: "Orders",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FpoAdminId",
                table: "Orders",
                column: "FpoAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryWeightLogs_OrderId",
                table: "DeliveryWeightLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryWeightLogs_RouteId",
                table: "DeliveryWeightLogs",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_BuyerId",
                table: "EscrowTransactions",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_OrderId",
                table: "EscrowTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerPayouts_FarmerId",
                table: "FarmerPayouts",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerPayouts_FpoAdminId",
                table: "FarmerPayouts",
                column: "FpoAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFees_EscrowId",
                table: "PlatformFees",
                column: "EscrowId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFees_OrderId",
                table: "PlatformFees",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLedgers_OrderId",
                table: "TransactionLedgers",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_FarmerId",
                table: "Orders",
                column: "FarmerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_FpoAdminId",
                table: "Orders",
                column: "FpoAdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_FarmerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_FpoAdminId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryWeightLogs");

            migrationBuilder.DropTable(
                name: "FarmerPayouts");

            migrationBuilder.DropTable(
                name: "PlatformFees");

            migrationBuilder.DropTable(
                name: "TransactionLedgers");

            migrationBuilder.DropTable(
                name: "EscrowTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FarmerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FpoAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CropName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryConfirmedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDateTarget",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FarmerAskingPricePerKg",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FarmerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FpoAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuantityDeliveredKg",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuantityOrderedKg",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuantityPickedUpKg",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Season",
                table: "Orders");
        }
    }
}
