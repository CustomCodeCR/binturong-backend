using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxLockAndNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_error",
                table: "OutboxMessages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_locked_until",
                table: "OutboxMessages",
                column: "locked_until");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_locked_until",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "locked_until",
                table: "OutboxMessages");

            migrationBuilder.AlterColumn<string>(
                name: "last_error",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
