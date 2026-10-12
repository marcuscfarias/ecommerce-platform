using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmailConfirmed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                schema: "auth",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                schema: "auth",
                table: "Users");
        }
    }
}
