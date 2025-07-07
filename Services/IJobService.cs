using DelayedQ.Models;
using DelayedQ.DTOs;

namespace DelayedQ.Services
{
    public interface IJobService
    {
        Task<JobResponse> CreateJobAsync(CreateJobRequest request);
        Task<JobResponse?> GetJobAsync(Guid id); // Keep for backward compatibility
        Task<JobResponse?> GetJobByEventIdAsync(string eventId);
        Task<JobResponse?> GetJobByEventIdAndTimestampAsync(string eventId, DateTime timestamp);
        Task<JobResponse?> UpdateJobAsync(string eventId, UpdateJobRequest request);
        Task<bool> CancelJobAsync(string eventId);
        Task<IEnumerable<JobResponse>> GetJobsAsync();
        Task<IEnumerable<JobResponse>> GetJobsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
