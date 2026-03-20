using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace binturong.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityStatusToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "availability_status",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "Services",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "category_name",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_category_protected",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "SalesOrderDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "item_type",
                table: "SalesOrderDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "service_id",
                table: "SalesOrderDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_category_id",
                table: "Services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_order_details_service_id",
                table: "SalesOrderDetails",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sales_order_details_services_service_id",
                table: "SalesOrderDetails",
                column: "service_id",
                principalTable: "Services",
                principalColumn: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "fk_services_product_categories_category_id",
                table: "Services",
                column: "category_id",
                principalTable: "ProductCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sales_order_details_services_service_id",
                table: "SalesOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "fk_services_product_categories_category_id",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "ix_services_category_id",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "ix_sales_order_details_service_id",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "availability_status",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "category_name",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "is_category_protected",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "item_type",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "service_id",
                table: "SalesOrderDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "SalesOrderDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
