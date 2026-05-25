using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "auth",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """UPDATE auth."Users" SET "Name" = "FirstName" || ' ' || "LastName" """);

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "auth",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "auth",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "auth",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "auth",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "auth",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }
    }
}
