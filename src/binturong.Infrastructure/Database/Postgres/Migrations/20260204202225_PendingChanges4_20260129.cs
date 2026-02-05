using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges4_20260129 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_transfers", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_transfers_branches_from_branch_id",
                        column: x => x.from_branch_id,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_transfers_branches_to_branch_id",
                        column: x => x.to_branch_id,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transfer_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_transfer_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_transfer_lines_inventory_transfers_transfer_id",
                        column: x => x.transfer_id,
                        principalTable: "inventory_transfers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_transfer_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_transfer_lines_warehouses_from_warehouse_id",
                        column: x => x.from_warehouse_id,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_transfer_lines_warehouses_to_warehouse_id",
                        column: x => x.to_warehouse_id,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfer_lines_from_warehouse_id",
                table: "inventory_transfer_lines",
                column: "from_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfer_lines_product_id",
                table: "inventory_transfer_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfer_lines_to_warehouse_id",
                table: "inventory_transfer_lines",
                column: "to_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfer_lines_transfer_id",
                table: "inventory_transfer_lines",
                column: "transfer_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfers_from_branch_id",
                table: "inventory_transfers",
                column: "from_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transfers_to_branch_id",
                table: "inventory_transfers",
                column: "to_branch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_transfer_lines");

            migrationBuilder.DropTable(
                name: "inventory_transfers");
        }
    }
}
