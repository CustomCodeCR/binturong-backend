using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "Invoices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notes",
                table: "Invoices");
        }
    }
}
