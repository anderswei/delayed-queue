using Microsoft.AspNetCore.Mvc;
using DelayedQ.DTOs;
using DelayedQ.Services;

namespace DelayedQ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartitionController : ControllerBase
    {
        private readonly IPartitionService _partitionService;
        private readonly ILogger<PartitionController> _logger;

        public PartitionController(IPartitionService partitionService, ILogger<PartitionController> logger)
        {
            _partitionService = partitionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PartitionResponse>> CreatePartition([FromBody] CreateDailyPartitionsRequest request)
        {
           if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _partitionService.CreateDailyPartitionsAsync(request.StartDate, request.NumberOfDays);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Daily partitions created successfully for {NumberOfDays} days starting from {StartDate}", 
                        request.NumberOfDays, request.StartDate);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to create daily partitions: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily partitions");
                return StatusCode(500, new PartitionResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while creating daily partitions"
                });
            }
        }

    }

    public class CreateDailyPartitionsRequest
    {
        public DateTime StartDate { get; set; }
        public int NumberOfDays { get; set; } = 7;
    }
}
