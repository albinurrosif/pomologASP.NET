using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomolog.Api.Migrations
{
    /// <inheritdoc />
    public partial class CodeRabbitFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionTaskRecords_TaskItems_TaskId",
                table: "SessionTaskRecords");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId",
                table: "PomodoroSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PomodoroSessions_Users_UserId",
                table: "PomodoroSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionTaskRecords_TaskItems_TaskId",
                table: "SessionTaskRecords",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PomodoroSessions_Users_UserId",
                table: "PomodoroSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionTaskRecords_TaskItems_TaskId",
                table: "SessionTaskRecords");

            migrationBuilder.DropIndex(
                name: "IX_PomodoroSessions_UserId",
                table: "PomodoroSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionTaskRecords_TaskItems_TaskId",
                table: "SessionTaskRecords",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
