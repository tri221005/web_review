using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace page.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeritageTimelineJson",
                table: "Destinations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandingSceneKey",
                table: "Destinations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Destinations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Destinations",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SavedItineraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredTypes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedItineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedItineraries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TravelStories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpvoteCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelStories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelStories_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRegionStamps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegionStamps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRegionStamps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitedDestinations",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    VisitedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitedDestinations", x => new { x.UserId, x.DestinationId });
                    table.ForeignKey(
                        name: "FK_VisitedDestinations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitedDestinations_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItineraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItineraryId = table.Column<int>(type: "int", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_SavedItineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "SavedItineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryUpvotes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryUpvotes", x => new { x.UserId, x.StoryId });
                    table.ForeignKey(
                        name: "FK_StoryUpvotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryUpvotes_TravelStories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "TravelStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_DestinationId",
                table: "ItineraryItems",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId",
                table: "ItineraryItems",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedItineraries_UserId",
                table: "SavedItineraries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryUpvotes_StoryId",
                table: "StoryUpvotes",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelStories_DestinationId",
                table: "TravelStories",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelStories_UserId",
                table: "TravelStories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegionStamps_UserId_Region",
                table: "UserRegionStamps",
                columns: new[] { "UserId", "Region" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitedDestinations_DestinationId",
                table: "VisitedDestinations",
                column: "DestinationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItineraryItems");

            migrationBuilder.DropTable(
                name: "StoryUpvotes");

            migrationBuilder.DropTable(
                name: "UserRegionStamps");

            migrationBuilder.DropTable(
                name: "VisitedDestinations");

            migrationBuilder.DropTable(
                name: "SavedItineraries");

            migrationBuilder.DropTable(
                name: "TravelStories");

            migrationBuilder.DropColumn(
                name: "HeritageTimelineJson",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "LandingSceneKey",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Destinations");
        }
    }
}
