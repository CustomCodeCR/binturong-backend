using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges_20260211 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "low_stock_alert_active",
                table: "WarehouseStock",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "low_stock_last_notified_at_utc",
                table: "WarehouseStock",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "auto_renew_enabled",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "auto_renew_every_days",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "expiry_alert_active",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "expiry_last_notified_at_utc",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expiry_notice_days",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "renewed_at_utc",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "responsible_user_id",
                table: "Contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "contract_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    uploaded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_attachments", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_attachments");

            migrationBuilder.DropColumn(
                name: "low_stock_alert_active",
                table: "WarehouseStock");

            migrationBuilder.DropColumn(
                name: "low_stock_last_notified_at_utc",
                table: "WarehouseStock");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "auto_renew_enabled",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "auto_renew_every_days",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "expiry_alert_active",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "expiry_last_notified_at_utc",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "expiry_notice_days",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "renewed_at_utc",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "responsible_user_id",
                table: "Contracts");
        }
    }
}
