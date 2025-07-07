using DelayedQ.DTOs;
using DelayedQ.Data;
using Microsoft.EntityFrameworkCore;

namespace DelayedQ.Services
{
    public interface IPartitionService
    {
        Task<PartitionResponse> CreatePartitionAsync(CreatePartitionRequest request);
        Task<ListPartitionsResponse> GetPartitionsAsync();
        Task<PartitionResponse> CreateDailyPartitionsAsync(DateTime startDate, int numberOfDays);
        Task<bool> DropPartitionAsync(string partitionName);
    }

    public class PartitionService : IPartitionService
    {
        private readonly DelayedQDbContext _context;
        private readonly ILogger<PartitionService> _logger;

        public PartitionService(DelayedQDbContext context, ILogger<PartitionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PartitionResponse> CreatePartitionAsync(CreatePartitionRequest request)
        {
            try
            {
                if (request.FromDate >= request.ToDate)
                {
                    return new PartitionResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "FromDate must be less than ToDate"
                    };
                }

                var createdPartitions = new List<string>();
                var skippedPartitions = new List<string>();
                var failedPartitions = new List<string>();

                // Always create 1 partition per day, loop through all days in the range
                var currentDate = request.FromDate.Date;
                var endDate = request.ToDate.Date;

                while (currentDate < endDate)
                {
                    var nextDate = currentDate.AddDays(1);
                    var partitionName = $"Jobs_{currentDate:yyyyMMdd}";

                    // Check if partition already exists
                    var existingPartition = await GetPartitionByNameAsync(partitionName);
                    if (existingPartition != null)
                    {
                        skippedPartitions.Add(partitionName);
                        _logger.LogInformation("Partition {PartitionName} already exists, skipping", partitionName);
                        currentDate = nextDate;
                        continue;
                    }

                    try
                    {
                        // Create daily partition
                        var sql = $@"
                            CREATE TABLE ""{partitionName}"" PARTITION OF ""Jobs""
                            FOR VALUES FROM ('{currentDate:yyyy-MM-dd 00:00:00}') TO ('{nextDate:yyyy-MM-dd 00:00:00}');
                        ";

                        await _context.Database.ExecuteSqlRawAsync(sql);
                        createdPartitions.Add(partitionName);
                        
                        _logger.LogInformation("Created daily partition {PartitionName} for date {Date}", 
                            partitionName, currentDate);
                    }
                    catch (Exception ex)
                    {
                        failedPartitions.Add(partitionName);
                        _logger.LogError(ex, "Failed to create partition {PartitionName}", partitionName);
                    }

                    currentDate = nextDate;
                }

                // Build response message
                var totalDays = (int)(endDate - request.FromDate.Date).TotalDays;
                var messages = new List<string>();
                if (createdPartitions.Any())
                    messages.Add($"Created: {string.Join(", ", createdPartitions)}");
                if (skippedPartitions.Any())
                    messages.Add($"Skipped (already exist): {string.Join(", ", skippedPartitions)}");
                if (failedPartitions.Any())
                    messages.Add($"Failed: {string.Join(", ", failedPartitions)}");

                var isSuccess = createdPartitions.Any() && !failedPartitions.Any();
                var errorMessage = failedPartitions.Any() ? $"Some partitions failed to create: {string.Join(", ", failedPartitions)}" : null;

                return new PartitionResponse
                {
                    PartitionName = string.Join(", ", createdPartitions),
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage ?? (createdPartitions.Any() ? null : "No partitions were created"),
                    TotalPartitionsRequested = totalDays,
                    PartitionsCreated = createdPartitions.Count,
                    PartitionsSkipped = skippedPartitions.Count,
                    PartitionsFailed = failedPartitions.Count,
                    CreatedPartitionNames = createdPartitions,
                    SkippedPartitionNames = skippedPartitions,
                    FailedPartitionNames = failedPartitions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partitions for range {FromDate} to {ToDate}", 
                    request.FromDate, request.ToDate);
                
                return new PartitionResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ListPartitionsResponse> GetPartitionsAsync()
        {
            try
            {
                var sql = @"
                    SELECT 
                        schemaname,
                        tablename,
                        pg_get_expr(pt.partexprs, pt.partrelid) as partition_expression,
                        pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as table_size
                    FROM pg_tables t
                    JOIN pg_class c ON c.relname = t.tablename
                    JOIN pg_partitioned_table pt ON pt.partrelid = c.oid
                    WHERE schemaname = 'public' 
                    AND tablename LIKE 'Jobs_%'
                    
                    UNION ALL
                    
                    SELECT 
                        schemaname,
                        tablename,
                        pg_get_expr(c.relpartbound, c.oid) as partition_expression,
                        pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as table_size
                    FROM pg_tables t
                    JOIN pg_class c ON c.relname = t.tablename
                    JOIN pg_inherits i ON i.inhrelid = c.oid
                    JOIN pg_class parent ON parent.oid = i.inhparent
                    WHERE schemaname = 'public' 
                    AND parent.relname = 'Jobs'
                    AND tablename LIKE 'Jobs_%'
                    ORDER BY tablename;
                ";

                var partitions = new List<PartitionInfo>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    await _context.Database.OpenConnectionAsync();
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var partition = new PartitionInfo
                            {
                                SchemaName = reader.GetString(0),
                                TableName = reader.GetString(1),
                                PartitionExpression = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                TableSize = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            };

                            // Parse dates from partition expression or table name
                            ParsePartitionDates(partition);
                            partitions.Add(partition);
                        }
                    }
                }

                return new ListPartitionsResponse
                {
                    Partitions = partitions,
                    TotalPartitions = partitions.Count,
                    QueryTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partitions");
                return new ListPartitionsResponse
                {
                    Partitions = new List<PartitionInfo>(),
                    TotalPartitions = 0,
                    QueryTime = DateTime.UtcNow
                };
            }
        }

        public async Task<PartitionResponse> CreateDailyPartitionsAsync(DateTime startDate, int numberOfDays)
        {
            try
            {
                // Use the same logic as CreatePartitionAsync by creating a date range
                var fromDate = startDate.Date;
                var toDate = fromDate.AddDays(numberOfDays);
                
                var request = new CreatePartitionRequest
                {
                    FromDate = fromDate,
                    ToDate = toDate
                };

                // Delegate to the main partition creation method
                return await CreatePartitionAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily partitions");
                return new PartitionResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> DropPartitionAsync(string partitionName)
        {
            try
            {
                var sql = $"DROP TABLE IF EXISTS \"{partitionName}\";";
                await _context.Database.ExecuteSqlRawAsync(sql);
                
                _logger.LogInformation("Dropped partition {PartitionName}", partitionName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping partition {PartitionName}", partitionName);
                return false;
            }
        }

        private async Task<PartitionInfo?> GetPartitionByNameAsync(string partitionName)
        {
            var sql = $@"
                SELECT tablename 
                FROM pg_tables 
                WHERE schemaname = 'public' 
                AND tablename = '{partitionName}';
            ";

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                await _context.Database.OpenConnectionAsync();
                
                var result = await command.ExecuteScalarAsync();
                return result != null ? new PartitionInfo { TableName = result.ToString()! } : null;
            }
        }

        private void ParsePartitionDates(PartitionInfo partition)
        {
            try
            {
                // Try to parse dates from table name pattern Jobs_YYYYMMDD
                if (partition.TableName.StartsWith("Jobs_") && partition.TableName.Length >= 13)
                {
                    var dateString = partition.TableName.Substring(5, 8);
                    if (DateTime.TryParseExact(dateString, "yyyyMMdd", null, 
                        System.Globalization.DateTimeStyles.None, out var fromDate))
                    {
                        partition.FromDate = fromDate;
                        partition.ToDate = fromDate.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse partition dates from {TableName}", partition.TableName);
            }
        }
    }
}
