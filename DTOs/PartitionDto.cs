using System.ComponentModel.DataAnnotations;

namespace DelayedQ.DTOs
{
    public class CreatePartitionRequest
    {
        [Required]
        public DateTime FromDate { get; set; }
        
        [Required]
        public DateTime ToDate { get; set; }
        
        public string? PartitionName { get; set; }
    }

    public class PartitionResponse
    {
        public string PartitionName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalPartitionsRequested { get; set; }
        public int PartitionsCreated { get; set; }
        public int PartitionsSkipped { get; set; }
        public int PartitionsFailed { get; set; }
        public List<string> CreatedPartitionNames { get; set; } = new();
        public List<string> SkippedPartitionNames { get; set; } = new();
        public List<string> FailedPartitionNames { get; set; } = new();
    }

    public class PartitionInfo
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string PartitionExpression { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long RowCount { get; set; }
        public string TableSize { get; set; } = string.Empty;
    }

    public class ListPartitionsResponse
    {
        public List<PartitionInfo> Partitions { get; set; } = new();
        public int TotalPartitions { get; set; }
        public DateTime QueryTime { get; set; }
    }
}
