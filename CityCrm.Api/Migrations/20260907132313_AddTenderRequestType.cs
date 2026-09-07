using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityCrm.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTenderRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "TenderRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "TenderRequests");
        }
    }
}
