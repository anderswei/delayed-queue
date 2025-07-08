using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DelayedQ.Models
{
    public class LowPrecisionJob
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
        /// Unix timestamp when the job should be executed (DynamoDB TTL)
        /// </summary>
        [Required]
        public long TtlTimestamp { get; set; }
        
        /// <summary>
        /// Human-readable target execution time
        /// </summary>
        [Required]
        public DateTime TargetExecutionTime { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExecutedAt { get; set; }
        
        public string? Status { get; set; } = "Pending";
        
        /// <summary>
        /// DynamoDB partition key (could be date-based for better distribution)
        /// </summary>
        public string PartitionKey { get; set; } = string.Empty;
        
        /// <summary>
        /// DynamoDB sort key (EventId)
        /// </summary>
        public string SortKey { get; set; } = string.Empty;
    }
}
