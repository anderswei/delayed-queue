using DelayedQ.DTOs;

namespace DelayedQ.Services
{
    public interface ILowPrecisionJobService
    {
        Task<LowPrecisionJobResponse> CreateLowPrecisionJobAsync(CreateLowPrecisionJobRequest request);
        Task<LowPrecisionJobResponse?> GetLowPrecisionJobByEventIdAsync(string eventId);
        Task<LowPrecisionJobResponse?> UpdateLowPrecisionJobAsync(string eventId, UpdateLowPrecisionJobRequest request);
        Task<bool> CancelLowPrecisionJobAsync(string eventId);
        Task<IEnumerable<LowPrecisionJobResponse>> GetLowPrecisionJobsByDateAsync(DateTime date);
    }
}
