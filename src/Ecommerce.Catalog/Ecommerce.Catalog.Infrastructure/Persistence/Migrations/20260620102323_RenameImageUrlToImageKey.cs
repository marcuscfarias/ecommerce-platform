using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageUrlToImageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "catalog",
                table: "Products",
                newName: "ImageKey");

            // Existing rows stored the full blob URL; keep only the blob name (last path segment).
            migrationBuilder.Sql(
                "UPDATE catalog.Products " +
                "SET ImageKey = RIGHT(ImageKey, CHARINDEX('/', REVERSE(ImageKey)) - 1) " +
                "WHERE ImageKey IS NOT NULL AND CHARINDEX('/', ImageKey) > 0;");

            migrationBuilder.AlterColumn<string>(
                name: "ImageKey",
                schema: "catalog",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageKey",
                schema: "catalog",
                table: "Products",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "ImageKey",
                schema: "catalog",
                table: "Products",
                newName: "ImageUrl");
        }
    }
}
