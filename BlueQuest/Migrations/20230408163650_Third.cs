using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueQuest.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RateOfSuccess",
                table: "Quests",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Quests",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalAttempts",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RateOfSuccess",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "TotalAttempts",
                table: "Quests");
        }
    }
}
