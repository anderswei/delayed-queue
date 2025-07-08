using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DelayedQ.Models;

namespace DelayedQ.DTOs
{
    public class CreateLowPrecisionJobRequest
    {
        [Required]
        public string EventId { get; set; } = string.Empty;
        
        [Required]
        public JsonElement CallbackPayload { get; set; }
        
        [Required]
        public CallbackType CallbackType { get; set; }
        
        [Required]
        [Url]
        public string CallbackUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Target execution time (will be converted to DynamoDB TTL timestamp)
        /// Note: DynamoDB TTL has a precision of up to 48 hours variance
        /// </summary>
        [Required]
        public DateTime TargetExecutionTime { get; set; }
    }

    public class LowPrecisionJobResponse
    {
        public string EventId { get; set; } = string.Empty;
        public JsonElement CallbackPayload { get; set; }
        public CallbackType CallbackType { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
        public DateTime TargetExecutionTime { get; set; }
        public long TtlTimestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public string? Status { get; set; }
        public string PartitionKey { get; set; } = string.Empty;
        public string SortKey { get; set; } = string.Empty;
    }

    public class UpdateLowPrecisionJobRequest
    {
        [Required]
        public JsonElement CallbackPayload { get; set; }
        
        [Required]
        public CallbackType CallbackType { get; set; }
        
        [Required]
        [Url]
        public string CallbackUrl { get; set; } = string.Empty;
        
        [Required]
        public DateTime TargetExecutionTime { get; set; }
        
        public string? Status { get; set; }
    }
}
