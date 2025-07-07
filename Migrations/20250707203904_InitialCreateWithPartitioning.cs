using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DelayedQ.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithPartitioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the Jobs table as a partitioned table
            migrationBuilder.Sql(@"
                CREATE TABLE ""Jobs"" (
                    ""EventId"" character varying(255) NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""CallbackPayload"" jsonb NOT NULL,
                    ""CallbackType"" text NOT NULL,
                    ""CallbackUrl"" character varying(2048) NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""ExecutedAt"" timestamp with time zone NULL,
                    ""Status"" character varying(50) NULL DEFAULT 'Pending',
                    CONSTRAINT ""PK_Jobs"" PRIMARY KEY (""EventId"", ""Timestamp"")
                ) PARTITION BY RANGE (""Timestamp"");
            ");

            // Create indexes on the partitioned table
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
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Jobs"" CASCADE;");
        }
    }
}
