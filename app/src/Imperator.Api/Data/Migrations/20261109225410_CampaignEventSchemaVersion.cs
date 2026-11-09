using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imperator.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CampaignEventSchemaVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "schema_version",
                table: "campaign_events",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "schema_version",
                table: "campaign_events");
        }
    }
}
