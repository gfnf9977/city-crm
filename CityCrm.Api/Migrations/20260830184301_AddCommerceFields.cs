using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCommerceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessCategory",
                table: "Premises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessDescription",
                table: "Premises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Premises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicVisible",
                table: "Premises",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHours",
                table: "Premises",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessCategory",
                table: "Premises");

            migrationBuilder.DropColumn(
                name: "BusinessDescription",
                table: "Premises");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Premises");

            migrationBuilder.DropColumn(
                name: "IsPublicVisible",
                table: "Premises");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                table: "Premises");
        }
    }
}
