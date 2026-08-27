using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:public.case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, collation: "case_insensitive"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "case_insensitive"),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, collation: "case_insensitive"),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, collation: "case_insensitive"),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive"),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ImageKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Categories",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Electronic devices and gadgets", true, "Electronics" },
                    { 2, "Apparel and fashion items", true, "Clothing" },
                    { 3, "Home decor and garden supplies", true, "Home & Garden" },
                    { 4, "Sports equipment and outdoor gear", true, "Sports & Outdoors" },
                    { 5, "Books, e-books, and audiobooks", true, "Books" },
                    { 6, "Health, beauty, and personal care products", true, "Health & Beauty" },
                    { 7, "Toys, games, and entertainment", true, "Toys & Games" },
                    { 8, "Food, drinks, and grocery items", true, "Food & Beverages" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageKey", "IsActive", "Name", "Sku", "StockQuantity", "Price", "Currency" },
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

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                schema: "catalog",
                table: "Products",
                column: "Sku",
                unique: true);

            // Seeded rows carry explicit keys, which leaves the identity sequences at 1.
            // Advance them past the seed so the first user-created row does not collide.
            migrationBuilder.Sql(
                """
                SELECT setval(pg_get_serial_sequence('catalog."Categories"', 'Id'), (SELECT MAX("Id") FROM catalog."Categories"));
                SELECT setval(pg_get_serial_sequence('catalog."Products"', 'Id'), (SELECT MAX("Id") FROM catalog."Products"));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "catalog");
        }
    }
}
