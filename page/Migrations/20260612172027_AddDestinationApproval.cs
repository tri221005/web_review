using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace page.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Destinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedByUserId",
                table: "Destinations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_SubmittedByUserId",
                table: "Destinations",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Destinations_AspNetUsers_SubmittedByUserId",
                table: "Destinations",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Destinations_AspNetUsers_SubmittedByUserId",
                table: "Destinations");

            migrationBuilder.DropIndex(
                name: "IX_Destinations_SubmittedByUserId",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Destinations");
        }
    }
}
