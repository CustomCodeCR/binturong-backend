using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges5_20260129 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventory_movements_inventory_movement_types_movement_type_id",
                table: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "InventoryMovementTypes");

            migrationBuilder.DropIndex(
                name: "ix_inventory_movements_movement_type_id",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "MovementTypeId",
                table: "InventoryMovements");

            migrationBuilder.AddColumn<int>(
                name: "movement_type",
                table: "InventoryMovements",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "movement_type",
                table: "InventoryMovements");

            migrationBuilder.AddColumn<Guid>(
                name: "MovementTypeId",
                table: "InventoryMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "InventoryMovementTypes",
                columns: table => new
                {
                    MovementTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sign = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_movement_types", x => x.MovementTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_movement_type_id",
                table: "InventoryMovements",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movement_types_code",
                table: "InventoryMovementTypes",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_movements_inventory_movement_types_movement_type_id",
                table: "InventoryMovements",
                column: "MovementTypeId",
                principalTable: "InventoryMovementTypes",
                principalColumn: "MovementTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
