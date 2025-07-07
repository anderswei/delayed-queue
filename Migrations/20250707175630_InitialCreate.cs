using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DelayedQ.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CallbackPayload = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    CallbackType = table.Column<string>(type: "text", nullable: false),
                    CallbackUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedAt",
                table: "Jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Timestamp",
                table: "Jobs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
