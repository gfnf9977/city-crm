using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceLinkToPremise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceLink",
                table: "Premises",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceLink",
                table: "Premises");
        }
    }
}
