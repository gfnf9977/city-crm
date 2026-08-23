using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildingBlock",
                table: "Buildings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingBlock",
                table: "Buildings");
        }
    }
}
