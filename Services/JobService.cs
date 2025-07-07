using DelayedQ.Models;
using DelayedQ.DTOs;

namespace DelayedQ.Services
{
    /// <summary>
    /// In-memory implementation of IJobService for testing purposes.
    /// This service stores jobs in memory and is not persistent.
    /// For production use, use DatabaseJobService instead.
    /// </summary>
    public class JobService : IJobService
    {
        // In-memory storage for demonstration purposes
        // In production, you would use a database
        private readonly List<Job> _jobs = new List<Job>();
        private readonly object _lock = new object();

        public Task<JobResponse> CreateJobAsync(CreateJobRequest request)
        {
            var job = new Job
            {
                EventId = request.EventId,
                CallbackPayload = request.CallbackPayload,
                CallbackType = request.CallbackType,
                CallbackUrl = request.CallbackUrl,
                Timestamp = request.Timestamp
            };

            lock (_lock)
            {
                _jobs.Add(job);
            }

            var response = new JobResponse
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

            return Task.FromResult(response);
        }

        public Task<JobResponse?> GetJobAsync(Guid id)
        {
            // Note: This method signature needs to be updated to use EventId instead of Guid
            // For now, converting to string to search by EventId
            return GetJobByEventIdAsync(id.ToString());
        }

        public Task<JobResponse?> GetJobByEventIdAsync(string eventId)
        {
            Job? job;
            lock (_lock)
            {
                job = _jobs.FirstOrDefault(j => j.EventId == eventId);
            }

            if (job == null)
                return Task.FromResult<JobResponse?>(null);

            var response = new JobResponse
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

            return Task.FromResult<JobResponse?>(response);
        }

        public Task<JobResponse?> GetJobByEventIdAndTimestampAsync(string eventId, DateTime timestamp)
        {
            Job? job;
            lock (_lock)
            {
                job = _jobs.FirstOrDefault(j => j.EventId == eventId && j.Timestamp == timestamp);
            }

            if (job == null)
                return Task.FromResult<JobResponse?>(null);

            var response = new JobResponse
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

            return Task.FromResult<JobResponse?>(response);
        }

        public Task<JobResponse?> UpdateJobAsync(string eventId, UpdateJobRequest request)
        {
            Job? job;
            lock (_lock)
            {
                job = _jobs.FirstOrDefault(j => j.EventId == eventId);
                
                if (job != null)
                {
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
                }
            }

            if (job == null)
                return Task.FromResult<JobResponse?>(null);

            var response = new JobResponse
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

            return Task.FromResult<JobResponse?>(response);
        }

        public Task<bool> CancelJobAsync(string eventId)
        {
            bool cancelled = false;
            
            lock (_lock)
            {
                var job = _jobs.FirstOrDefault(j => j.EventId == eventId);
                
                if (job != null)
                {
                    // Check if job is already executed or completed
                    if (job.Status?.ToLower() == "executed" || job.Status?.ToLower() == "completed")
                    {
                        cancelled = false; // Cannot cancel already executed jobs
                    }
                    else
                    {
                        // Update job status to cancelled
                        job.Status = "Cancelled";
                        job.ExecutedAt = DateTime.UtcNow; // Mark when it was cancelled
                        cancelled = true;
                    }
                }
            }

            return Task.FromResult(cancelled);
        }

        public Task<IEnumerable<JobResponse>> GetJobsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            List<Job> jobsCopy;
            lock (_lock)
            {
                jobsCopy = new List<Job>(_jobs.Where(j => j.Timestamp >= fromDate && j.Timestamp < toDate));
            }

            var responses = jobsCopy.Select(job => new JobResponse
            {
                EventId = job.EventId,
                CallbackPayload = job.CallbackPayload,
                CallbackType = job.CallbackType,
                CallbackUrl = job.CallbackUrl,
                Timestamp = job.Timestamp,
                CreatedAt = job.CreatedAt,
                ExecutedAt = job.ExecutedAt,
                Status = job.Status
            });

            return Task.FromResult(responses);
        }

        public Task<IEnumerable<JobResponse>> GetJobsAsync()
        {
            List<Job> jobsCopy;
            lock (_lock)
            {
                jobsCopy = new List<Job>(_jobs);
            }

            var responses = jobsCopy.Select(job => new JobResponse
            {
                EventId = job.EventId,
                CallbackPayload = job.CallbackPayload,
                CallbackType = job.CallbackType,
                CallbackUrl = job.CallbackUrl,
                Timestamp = job.Timestamp,
                CreatedAt = job.CreatedAt,
                ExecutedAt = job.ExecutedAt,
                Status = job.Status
            });

            return Task.FromResult(responses);
        }
    }
}
