using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueQuest.Migrations
{
    /// <inheritdoc />
    public partial class sixth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quests_CreatedById",
                table: "Quests",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_Users_CreatedById",
                table: "Quests",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quests_Users_CreatedById",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_CreatedById",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Quests");
        }
    }
}
