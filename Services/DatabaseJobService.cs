using Microsoft.EntityFrameworkCore;
using DelayedQ.Models;
using DelayedQ.DTOs;
using DelayedQ.Data;

namespace DelayedQ.Services
{
    public class DatabaseJobService : IJobService
    {
        private readonly DelayedQDbContext _context;

        public DatabaseJobService(DelayedQDbContext context)
        {
            _context = context;
        }

        public async Task<JobResponse> CreateJobAsync(CreateJobRequest request)
        {
            // Check if job with same EventId already exists
            var existingJob = await _context.Jobs.FirstOrDefaultAsync(j => j.EventId == request.EventId);
            if (existingJob != null)
            {
                throw new InvalidOperationException($"Job with EventId '{request.EventId}' already exists");
            }

            var job = new Job
            {
                EventId = request.EventId,
                CallbackPayload = request.CallbackPayload,
                CallbackType = request.CallbackType,
                CallbackUrl = request.CallbackUrl,
                Timestamp = request.Timestamp
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobResponse
            {
                EventId = job.EventId,
                CallbackPayload = job.CallbackPayload,
                CallbackType = job.CallbackType,
                CallbackUrl = job.CallbackUrl,
                Timestamp = job.Timestamp,
                CreatedAt = job.CreatedAt,
                ExecutedAt = job.ExecutedAt,
                Status = job.Status
            };
        }

        public async Task<JobResponse?> GetJobAsync(Guid id)
        {
            // For backward compatibility, convert GUID to string and search by EventId
            return await GetJobByEventIdAsync(id.ToString());
        }

        public async Task<JobResponse?> GetJobByEventIdAsync(string eventId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.EventId == eventId);
            
            if (job == null)
                return null;

            return MapToResponse(job);
        }

        public async Task<JobResponse?> GetJobByEventIdAndTimestampAsync(string eventId, DateTime timestamp)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.EventId == eventId && j.Timestamp == timestamp);
            
            if (job == null)
                return null;

            return MapToResponse(job);
        }

        public async Task<JobResponse?> UpdateJobAsync(string eventId, UpdateJobRequest request)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.EventId == eventId);
            
            if (job == null)
                return null;

            // Update the job properties
            job.CallbackPayload = request.CallbackPayload;
            job.CallbackType = request.CallbackType;
            job.CallbackUrl = request.CallbackUrl;
            job.Timestamp = request.Timestamp;
            
            if (!string.IsNullOrEmpty(request.Status))
            {
                job.Status = request.Status;
            }

            // If status is being set to executed, update ExecutedAt
            if (request.Status?.ToLower() == "executed" || request.Status?.ToLower() == "completed")
            {
                job.ExecutedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return MapToResponse(job);
        }

        public async Task<bool> CancelJobAsync(string eventId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.EventId == eventId);
            
            if (job == null)
                return false;

            // Check if job is already executed or completed
            if (job.Status?.ToLower() == "executed" || job.Status?.ToLower() == "completed")
            {
                return false; // Cannot cancel already executed jobs
            }

            // Update job status to cancelled
            job.Status = "Cancelled";
            job.ExecutedAt = DateTime.UtcNow; // Mark when it was cancelled

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<JobResponse>> GetJobsAsync()
        {
            var jobs = await _context.Jobs
                .OrderBy(j => j.Timestamp)
                .ToListAsync();

            return jobs.Select(MapToResponse);
        }

        public async Task<IEnumerable<JobResponse>> GetJobsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var jobs = await _context.Jobs
                .Where(j => j.Timestamp >= fromDate && j.Timestamp < toDate)
                .OrderBy(j => j.Timestamp)
                .ToListAsync();

            return jobs.Select(MapToResponse);
        }

        private JobResponse MapToResponse(Job job)
        {
            return new JobResponse
            {
                EventId = job.EventId,
                CallbackPayload = job.CallbackPayload,
                CallbackType = job.CallbackType,
                CallbackUrl = job.CallbackUrl,
                Timestamp = job.Timestamp,
                CreatedAt = job.CreatedAt,
                ExecutedAt = job.ExecutedAt,
                Status = job.Status
            };
        }
    }
}
