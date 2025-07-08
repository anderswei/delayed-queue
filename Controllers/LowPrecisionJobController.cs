using Microsoft.AspNetCore.Mvc;
using DelayedQ.DTOs;
using DelayedQ.Services;
using System.ComponentModel.DataAnnotations;

namespace DelayedQ.Controllers
{
    [ApiController]
    [Route("low-precision-job")]
    public class LowPrecisionJobController : ControllerBase
    {
        private readonly ILowPrecisionJobService _lowPrecisionJobService;
        private readonly ILogger<LowPrecisionJobController> _logger;

        public LowPrecisionJobController(ILowPrecisionJobService lowPrecisionJobService, ILogger<LowPrecisionJobController> logger)
        {
            _lowPrecisionJobService = lowPrecisionJobService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new low-precision job using DynamoDB TTL mechanism
        /// Note: DynamoDB TTL has a precision variance of up to 48 hours
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LowPrecisionJobResponse>> CreateLowPrecisionJob([FromBody] CreateLowPrecisionJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that target execution time is in the future
            if (request.TargetExecutionTime <= DateTime.UtcNow)
            {
                return BadRequest("TargetExecutionTime must be in the future");
            }

            try
            {
                var jobResponse = await _lowPrecisionJobService.CreateLowPrecisionJobAsync(request);
                _logger.LogInformation("Low-precision job created successfully with EventId: {EventId}", jobResponse.EventId);
                
                return CreatedAtAction(nameof(GetLowPrecisionJobByEventId), new { eventId = jobResponse.EventId }, jobResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating low-precision job");
                return StatusCode(500, "An error occurred while creating the low-precision job");
            }
        }

        /// <summary>
        /// Retrieves a low-precision job by EventId
        /// </summary>
        [HttpGet("{eventId}")]
        public async Task<ActionResult<LowPrecisionJobResponse>> GetLowPrecisionJobByEventId(string eventId)
        {
            try
            {
                var job = await _lowPrecisionJobService.GetLowPrecisionJobByEventIdAsync(eventId);
                if (job == null)
                {
                    return NotFound($"Low-precision job with EventId {eventId} not found");
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low-precision job with EventId: {EventId}", eventId);
                return StatusCode(500, "An error occurred while retrieving the low-precision job");
            }
        }

        /// <summary>
        /// Updates a low-precision job
        /// </summary>
        [HttpPut("{eventId}")]
        public async Task<ActionResult<LowPrecisionJobResponse>> UpdateLowPrecisionJob(string eventId, [FromBody] UpdateLowPrecisionJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that target execution time is in the future
            if (request.TargetExecutionTime <= DateTime.UtcNow)
            {
                return BadRequest("TargetExecutionTime must be in the future");
            }

            try
            {
                var updatedJob = await _lowPrecisionJobService.UpdateLowPrecisionJobAsync(eventId, request);
                if (updatedJob == null)
                {
                    return NotFound($"Low-precision job with EventId {eventId} not found or cannot be updated");
                }

                _logger.LogInformation("Low-precision job updated successfully with EventId: {EventId}", eventId);
                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating low-precision job with EventId: {EventId}", eventId);
                return StatusCode(500, "An error occurred while updating the low-precision job");
            }
        }

        /// <summary>
        /// Cancels a low-precision job
        /// </summary>
        [HttpDelete("{eventId}")]
        public async Task<ActionResult> CancelLowPrecisionJob(string eventId)
        {
            try
            {
                var cancelled = await _lowPrecisionJobService.CancelLowPrecisionJobAsync(eventId);
                
                if (!cancelled)
                {
                    var job = await _lowPrecisionJobService.GetLowPrecisionJobByEventIdAsync(eventId);
                    if (job == null)
                    {
                        return NotFound($"Low-precision job with EventId {eventId} not found");
                    }
                    else
                    {
                        return BadRequest($"Low-precision job with EventId {eventId} cannot be cancelled. It may already be executed or completed.");
                    }
                }

                _logger.LogInformation("Low-precision job cancelled successfully with EventId: {EventId}", eventId);
                return Ok(new { success = true, message = $"Low-precision job with EventId {eventId} has been cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling low-precision job with EventId: {EventId}", eventId);
                return StatusCode(500, "An error occurred while cancelling the low-precision job");
            }
        }

        /// <summary>
        /// Retrieves all low-precision jobs for a specific date
        /// </summary>
        [HttpGet("by-date/{date}")]
        public async Task<ActionResult<IEnumerable<LowPrecisionJobResponse>>> GetLowPrecisionJobsByDate(DateTime date)
        {
            try
            {
                var jobs = await _lowPrecisionJobService.GetLowPrecisionJobsByDateAsync(date);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low-precision jobs for date: {Date}", date);
                return StatusCode(500, "An error occurred while retrieving low-precision jobs");
            }
        }
    }
}
