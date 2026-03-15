using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges1_20260314 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "UserRoles");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "user_roles");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_roles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "user_roles",
                newName: "id");

            migrationBuilder.AddColumn<Guid>(
                name: "role_id1",
                table: "user_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "user_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id1",
                table: "user_roles",
                column: "role_id1");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                table: "user_roles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id1",
                table: "user_roles",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id1",
                table: "user_roles",
                column: "role_id1",
                principalTable: "Roles",
                principalColumn: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id1",
                table: "user_roles",
                column: "user_id1",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id1",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id1",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_role_id1",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_user_id",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_user_id1",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "role_id1",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "user_roles");

            migrationBuilder.RenameTable(
                name: "user_roles",
                newName: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserRoles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "UserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UserRoles",
                newName: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }
    }
}
