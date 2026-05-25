using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlugFromCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "catalog",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "catalog",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Slug",
                value: "electronics");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Slug",
                value: "clothing");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Slug",
                value: "home-garden");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Slug",
                value: "sports-outdoors");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Slug",
                value: "books");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Slug",
                value: "health-beauty");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Slug",
                value: "toys-games");

            migrationBuilder.UpdateData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "Slug",
                value: "food-beverages");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                schema: "catalog",
                table: "Categories",
                column: "Slug",
                unique: true);
        }
    }
}
