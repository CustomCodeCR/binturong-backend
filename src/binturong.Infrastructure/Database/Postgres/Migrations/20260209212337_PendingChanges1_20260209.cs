using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges1_20260209 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplier_evaluations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    classification = table.Column<string>(type: "text", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    evaluated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_evaluations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_quotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    supplier_message = table.Column<string>(type: "text", nullable: false),
                    reject_reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_quote_line",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_quote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_quote_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_quote_line_supplier_quotes_supplier_quote_id",
                        column: x => x.supplier_quote_id,
                        principalTable: "supplier_quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_quote_line_supplier_quote_id",
                table: "supplier_quote_line",
                column: "supplier_quote_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_evaluations");

            migrationBuilder.DropTable(
                name: "supplier_quote_line");

            migrationBuilder.DropTable(
                name: "supplier_quotes");
        }
    }
}
