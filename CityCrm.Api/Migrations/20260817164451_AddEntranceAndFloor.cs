using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEntranceAndFloor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Entrance",
                table: "Premises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Premises",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entrance",
                table: "Premises");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Premises");
        }
    }
}
