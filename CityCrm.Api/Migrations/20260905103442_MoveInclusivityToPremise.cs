using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoveInclusivityToPremise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInclusive",
                table: "Buildings");

            migrationBuilder.AddColumn<bool>(
                name: "IsInclusive",
                table: "Premises",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInclusive",
                table: "Premises");

            migrationBuilder.AddColumn<bool>(
                name: "IsInclusive",
                table: "Buildings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
