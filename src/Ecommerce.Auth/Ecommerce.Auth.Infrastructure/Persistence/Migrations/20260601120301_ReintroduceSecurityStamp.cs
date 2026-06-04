using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReintroduceSecurityStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "auth",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """UPDATE auth."Users" SET "SecurityStamp" = replace(gen_random_uuid()::text, '-', '')""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "auth",
                table: "Users");
        }
    }
}
