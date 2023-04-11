using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlueQuest.Migrations
{
    /// <inheritdoc />
    public partial class Fourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Quests",
                newName: "UsersRating");

            migrationBuilder.AddColumn<int>(
                name: "QuestId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuccessfulAttempts",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_QuestId",
                table: "Users",
                column: "QuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Quests_QuestId",
                table: "Users",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Quests_QuestId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_QuestId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QuestId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SuccessfulAttempts",
                table: "Quests");

            migrationBuilder.RenameColumn(
                name: "UsersRating",
                table: "Quests",
                newName: "Rating");
        }
    }
}
