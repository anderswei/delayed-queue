using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DelayedQ.Models
{
    public enum CallbackType
    {
        HTTP,
        SQS
    }

    public class Job
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
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExecutedAt { get; set; }
        
        public string? Status { get; set; } = "Pending";
    }
}
