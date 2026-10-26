using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KeyCartsByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM orders.\"Carts\";");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                schema: "orders",
                table: "Carts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_CustomerId",
                schema: "orders",
                table: "Carts",
                newName: "IX_Carts_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "orders",
                table: "Carts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_UserId",
                schema: "orders",
                table: "Carts",
                newName: "IX_Carts_CustomerId");
        }
    }
}
