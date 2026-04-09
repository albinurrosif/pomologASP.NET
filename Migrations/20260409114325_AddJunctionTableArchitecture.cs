using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pomolog.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddJunctionTableArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedAtUtc",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "TotalPausedSeconds",
                table: "TaskItems");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TaskItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionTaskRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    MinutesSpent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTaskRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTaskRecords_PomodoroSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "PomodoroSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionTaskRecords_TaskItems_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTaskRecords_SessionId",
                table: "SessionTaskRecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTaskRecords_TaskId",
                table: "SessionTaskRecords",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionTaskRecords");

            migrationBuilder.DropTable(
                name: "PomodoroSessions");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TaskItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAtUtc",
                table: "TaskItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "TaskItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPausedSeconds",
                table: "TaskItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
