using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DelayedQ.Models;

namespace DelayedQ.DTOs
{
    public class CreateJobRequest
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
    }

    public class JobResponse
    {
        public string EventId { get; set; } = string.Empty;
        public JsonElement CallbackPayload { get; set; }
        public CallbackType CallbackType { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateJobRequest
    {
        [Required]
        public JsonElement CallbackPayload { get; set; }
        
        [Required]
        public CallbackType CallbackType { get; set; }
        
        [Required]
        [Url]
        public string CallbackUrl { get; set; } = string.Empty;
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        public string? Status { get; set; }
    }
}
