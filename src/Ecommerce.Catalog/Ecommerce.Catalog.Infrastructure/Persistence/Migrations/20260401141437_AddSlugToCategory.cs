using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "catalog",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                schema: "catalog",
                table: "Categories",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
