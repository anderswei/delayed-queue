using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DelayedQ.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePartitioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, we need to recreate the Jobs table as a partitioned table
            // Since we can't alter an existing table to be partitioned, we'll create a new one
            
            // Step 1: Rename the existing table
            migrationBuilder.Sql("ALTER TABLE \"Jobs\" RENAME TO \"Jobs_old\";");
            
            // Step 2: Create the new partitioned table
            migrationBuilder.Sql(@"
                CREATE TABLE ""Jobs"" (
                    ""EventId"" character varying(255) NOT NULL,
                    ""CallbackPayload"" jsonb NOT NULL,
                    ""CallbackType"" text NOT NULL,
                    ""CallbackUrl"" character varying(2048) NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""ExecutedAt"" timestamp with time zone NULL,
                    ""Status"" character varying(50) NULL DEFAULT 'Pending',
                    CONSTRAINT ""PK_Jobs"" PRIMARY KEY (""EventId"", ""Timestamp"")
                ) PARTITION BY RANGE (""Timestamp"");
            ");
            
            // Step 3: Create indexes on the partitioned table
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Jobs_CreatedAt"" ON ""Jobs"" (""CreatedAt"");
                CREATE INDEX ""IX_Jobs_Status"" ON ""Jobs"" (""Status"");
                CREATE INDEX ""IX_Jobs_Timestamp"" ON ""Jobs"" (""Timestamp"");
            ");
            
            // Step 4: Copy data from old table to new partitioned table
            // First create a default partition for existing data
            migrationBuilder.Sql(@"
                CREATE TABLE ""Jobs_default"" PARTITION OF ""Jobs"" DEFAULT;
            ");
            
            // Copy existing data
            migrationBuilder.Sql(@"
                INSERT INTO ""Jobs"" SELECT * FROM ""Jobs_old"";
            ");
            
            // Step 5: Drop the old table
            migrationBuilder.Sql("DROP TABLE \"Jobs_old\";");
            
            // Step 6: Create initial daily partitions for the next 7 days
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < 7; i++)
            {
                var partitionDate = today.AddDays(i);
                var nextDate = partitionDate.AddDays(1);
                var partitionName = $"Jobs_{partitionDate:yyyyMMdd}";
                
                migrationBuilder.Sql($@"
                    CREATE TABLE ""{partitionName}"" PARTITION OF ""Jobs""
                    FOR VALUES FROM ('{partitionDate:yyyy-MM-dd}') TO ('{nextDate:yyyy-MM-dd}');
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // To rollback partitioning, we need to recreate the original table structure
            
            // Step 1: Create a temporary table with the original structure
            migrationBuilder.Sql(@"
                CREATE TABLE ""Jobs_temp"" (
                    ""EventId"" character varying(255) NOT NULL,
                    ""CallbackPayload"" jsonb NOT NULL,
                    ""CallbackType"" text NOT NULL,
                    ""CallbackUrl"" character varying(2048) NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""ExecutedAt"" timestamp with time zone NULL,
                    ""Status"" character varying(50) NULL DEFAULT 'Pending',
                    CONSTRAINT ""PK_Jobs_temp"" PRIMARY KEY (""EventId"")
                );
            ");
            
            // Step 2: Copy data from partitioned table
            migrationBuilder.Sql(@"
                INSERT INTO ""Jobs_temp"" SELECT * FROM ""Jobs"";
            ");
            
            // Step 3: Drop the partitioned table (this will drop all partitions)
            migrationBuilder.Sql("DROP TABLE \"Jobs\" CASCADE;");
            
            // Step 4: Rename temp table to Jobs
            migrationBuilder.Sql("ALTER TABLE \"Jobs_temp\" RENAME TO \"Jobs\";");
            migrationBuilder.Sql("ALTER TABLE \"Jobs\" RENAME CONSTRAINT \"PK_Jobs_temp\" TO \"PK_Jobs\";");
            
            // Step 5: Recreate indexes
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Jobs_CreatedAt"" ON ""Jobs"" (""CreatedAt"");
                CREATE INDEX ""IX_Jobs_Status"" ON ""Jobs"" (""Status"");
                CREATE INDEX ""IX_Jobs_Timestamp"" ON ""Jobs"" (""Timestamp"");
            ");
        }
    }
}
