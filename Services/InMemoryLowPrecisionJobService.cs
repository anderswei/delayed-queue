using DelayedQ.DTOs;
using DelayedQ.Models;
using System.Collections.Concurrent;

namespace DelayedQ.Services
{
    public class InMemoryLowPrecisionJobService : ILowPrecisionJobService
    {
        private readonly ConcurrentDictionary<string, LowPrecisionJob> _lowPrecisionJobs = new();
        private readonly ILogger<InMemoryLowPrecisionJobService> _logger;

        public InMemoryLowPrecisionJobService(ILogger<InMemoryLowPrecisionJobService> logger)
        {
            _logger = logger;
        }

        public Task<LowPrecisionJobResponse> CreateLowPrecisionJobAsync(CreateLowPrecisionJobRequest request)
        {
            _logger.LogInformation("Creating low-precision job with EventId: {EventId}", request.EventId);

            var ttlTimestamp = ((DateTimeOffset)request.TargetExecutionTime).ToUnixTimeSeconds();
            var partitionKey = request.TargetExecutionTime.ToString("yyyy-MM-dd");
            
            var lowPrecisionJob = new LowPrecisionJob
            {
                EventId = request.EventId,
                CallbackPayload = request.CallbackPayload,
                CallbackType = request.CallbackType,
                CallbackUrl = request.CallbackUrl,
                TargetExecutionTime = request.TargetExecutionTime,
                TtlTimestamp = ttlTimestamp,
                CreatedAt = DateTime.UtcNow,
                PartitionKey = partitionKey,
                SortKey = request.EventId
            };

            _lowPrecisionJobs[request.EventId] = lowPrecisionJob;

            var response = MapToResponse(lowPrecisionJob);
            _logger.LogInformation("Low-precision job created successfully with EventId: {EventId}, TTL: {TtlTimestamp}", 
                request.EventId, ttlTimestamp);
            
            return Task.FromResult(response);
        }

        public Task<LowPrecisionJobResponse?> GetLowPrecisionJobByEventIdAsync(string eventId)
        {
            _logger.LogInformation("Retrieving low-precision job with EventId: {EventId}", eventId);

            if (_lowPrecisionJobs.TryGetValue(eventId, out var lowPrecisionJob))
            {
                return Task.FromResult<LowPrecisionJobResponse?>(MapToResponse(lowPrecisionJob));
            }

            _logger.LogWarning("Low-precision job with EventId: {EventId} not found", eventId);
            return Task.FromResult<LowPrecisionJobResponse?>(null);
        }

        public Task<LowPrecisionJobResponse?> UpdateLowPrecisionJobAsync(string eventId, UpdateLowPrecisionJobRequest request)
        {
            _logger.LogInformation("Updating low-precision job with EventId: {EventId}", eventId);

            if (!_lowPrecisionJobs.TryGetValue(eventId, out var existingJob))
            {
                _logger.LogWarning("Low-precision job with EventId: {EventId} not found for update", eventId);
                return Task.FromResult<LowPrecisionJobResponse?>(null);
            }

            // Check if job is already executed
            if (existingJob.Status?.ToLower() == "executed" || existingJob.Status?.ToLower() == "completed")
            {
                _logger.LogWarning("Cannot update low-precision job with EventId: {EventId} - already executed", eventId);
                return Task.FromResult<LowPrecisionJobResponse?>(null);
            }

            var ttlTimestamp = ((DateTimeOffset)request.TargetExecutionTime).ToUnixTimeSeconds();
            var partitionKey = request.TargetExecutionTime.ToString("yyyy-MM-dd");

            existingJob.CallbackPayload = request.CallbackPayload;
            existingJob.CallbackType = request.CallbackType;
            existingJob.CallbackUrl = request.CallbackUrl;
            existingJob.TargetExecutionTime = request.TargetExecutionTime;
            existingJob.TtlTimestamp = ttlTimestamp;
            existingJob.PartitionKey = partitionKey;
            existingJob.Status = request.Status ?? existingJob.Status;

            var response = MapToResponse(existingJob);
            _logger.LogInformation("Low-precision job updated successfully with EventId: {EventId}", eventId);
            
            return Task.FromResult<LowPrecisionJobResponse?>(response);
        }

        public Task<bool> CancelLowPrecisionJobAsync(string eventId)
        {
            _logger.LogInformation("Cancelling low-precision job with EventId: {EventId}", eventId);

            if (!_lowPrecisionJobs.TryGetValue(eventId, out var existingJob))
            {
                _logger.LogWarning("Low-precision job with EventId: {EventId} not found for cancellation", eventId);
                return Task.FromResult(false);
            }

            // Check if job is already executed
            if (existingJob.Status?.ToLower() == "executed" || existingJob.Status?.ToLower() == "completed")
            {
                _logger.LogWarning("Cannot cancel low-precision job with EventId: {EventId} - already executed", eventId);
                return Task.FromResult(false);
            }

            existingJob.Status = "Cancelled";
            existingJob.ExecutedAt = DateTime.UtcNow;

            _logger.LogInformation("Low-precision job cancelled successfully with EventId: {EventId}", eventId);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<LowPrecisionJobResponse>> GetLowPrecisionJobsByDateAsync(DateTime date)
        {
            _logger.LogInformation("Retrieving low-precision jobs for date: {Date}", date.ToString("yyyy-MM-dd"));

            var partitionKey = date.ToString("yyyy-MM-dd");
            var jobs = _lowPrecisionJobs.Values
                .Where(j => j.PartitionKey == partitionKey)
                .Select(MapToResponse)
                .ToList();

            _logger.LogInformation("Found {Count} low-precision jobs for date: {Date}", jobs.Count, date.ToString("yyyy-MM-dd"));
            return Task.FromResult<IEnumerable<LowPrecisionJobResponse>>(jobs);
        }

        private static LowPrecisionJobResponse MapToResponse(LowPrecisionJob lowPrecisionJob)
        {
            return new LowPrecisionJobResponse
            {
                EventId = lowPrecisionJob.EventId,
                CallbackPayload = lowPrecisionJob.CallbackPayload,
                CallbackType = lowPrecisionJob.CallbackType,
                CallbackUrl = lowPrecisionJob.CallbackUrl,
                TargetExecutionTime = lowPrecisionJob.TargetExecutionTime,
                TtlTimestamp = lowPrecisionJob.TtlTimestamp,
                CreatedAt = lowPrecisionJob.CreatedAt,
                ExecutedAt = lowPrecisionJob.ExecutedAt,
                Status = lowPrecisionJob.Status,
                PartitionKey = lowPrecisionJob.PartitionKey,
                SortKey = lowPrecisionJob.SortKey
            };
        }
    }
}
