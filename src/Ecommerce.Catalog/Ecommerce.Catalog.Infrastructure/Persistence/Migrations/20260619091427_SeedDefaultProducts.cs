using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "IsActive", "Name", "Sku", "StockQuantity", "Price", "Currency" },
                values: new object[,]
                {
                    { 1, 1, "Over-ear noise-cancelling wireless headphones.", null, true, "Wireless Headphones", "ELEC-WH-001", 0, 149.99m, "USD" },
                    { 2, 1, "Waterproof 4K action camera with image stabilization.", null, false, "4K Action Camera", "ELEC-AC-002", 0, 299.00m, "USD" },
                    { 3, 2, "Unisex classic-fit denim jacket.", null, true, "Classic Denim Jacket", "CLOT-DJ-003", 0, 79.90m, "USD" },
                    { 4, 2, "100% cotton crew-neck t-shirt.", null, false, "Cotton Crew T-Shirt", "CLOT-TS-004", 0, 19.99m, "USD" },
                    { 5, 3, "10-piece stainless steel cookware set.", null, true, "Stainless Steel Cookware Set", "HOME-CW-005", 0, 189.99m, "USD" },
                    { 6, 3, "Handmade ceramic plant pot with drainage.", null, false, "Ceramic Plant Pot", "HOME-PP-006", 0, 24.50m, "USD" },
                    { 7, 4, "Vacuum-insulated 750ml stainless steel bottle.", null, true, "Insulated Water Bottle", "SPRT-WB-007", 0, 29.99m, "USD" },
                    { 8, 4, "Non-slip 6mm yoga mat with carrying strap.", null, false, "Yoga Mat Pro", "SPRT-YM-008", 0, 45.00m, "USD" },
                    { 9, 5, "A craftsman's guide to software structure and design.", null, true, "Clean Architecture", "BOOK-CA-009", 0, 39.99m, "USD" },
                    { 10, 5, "Your journey to mastery, 20th anniversary edition.", null, false, "The Pragmatic Programmer", "BOOK-PP-010", 0, 42.50m, "USD" },
                    { 11, 6, "Brightening facial serum with hyaluronic acid.", null, true, "Vitamin C Serum", "HLTH-VC-011", 0, 27.99m, "USD" },
                    { 12, 6, "Rechargeable sonic toothbrush with timer.", null, false, "Electric Toothbrush", "HLTH-ET-012", 0, 59.99m, "USD" },
                    { 13, 7, "500-piece creative building blocks set.", null, true, "Building Blocks Set", "TOYS-BB-013", 0, 34.99m, "USD" },
                    { 14, 7, "Award-winning strategy board game for 2-4 players.", null, false, "Strategy Board Game", "TOYS-BG-014", 0, 49.99m, "USD" },
                    { 15, 8, "Single-origin medium-roast whole coffee beans, 1kg.", null, true, "Gourmet Coffee Beans", "FOOD-CB-015", 0, 18.99m, "USD" },
                    { 16, 8, "Loose-leaf organic green tea, 200g.", null, false, "Organic Green Tea", "FOOD-GT-016", 0, 12.99m, "USD" },
                    { 17, 5, "Personalized handwritten letter on premium paper.", null, true, "Letter", "BOOK-LT-017", 50, 5.00m, "USD" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
