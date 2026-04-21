using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueIndexOnCategoryName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Categories",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Electronic devices and gadgets", true, "Electronics", "electronics" },
                    { 2, "Apparel and fashion items", true, "Clothing", "clothing" },
                    { 3, "Home decor and garden supplies", true, "Home & Garden", "home-garden" },
                    { 4, "Sports equipment and outdoor gear", true, "Sports & Outdoors", "sports-outdoors" },
                    { 5, "Books, e-books, and audiobooks", true, "Books", "books" },
                    { 6, "Health, beauty, and personal care products", true, "Health & Beauty", "health-beauty" },
                    { 7, "Toys, games, and entertainment", true, "Toys & Games", "toys-games" },
                    { 8, "Food, drinks, and grocery items", true, "Food & Beverages", "food-beverages" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                schema: "catalog",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
