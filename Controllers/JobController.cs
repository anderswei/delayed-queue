using Microsoft.AspNetCore.Mvc;
using DelayedQ.DTOs;
using DelayedQ.Services;
using System.ComponentModel.DataAnnotations;

namespace DelayedQ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobController> _logger;

        public JobController(IJobService jobService, ILogger<JobController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<JobResponse>> CreateJob([FromBody] CreateJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var jobResponse = await _jobService.CreateJobAsync(request);
                _logger.LogInformation("Job created successfully with EventId: {EventId}", jobResponse.EventId);
                
                return CreatedAtAction(nameof(GetJobByEventId), new { eventId = jobResponse.EventId }, jobResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                return StatusCode(500, "An error occurred while creating the job");
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<JobResponse>> GetJobByEventId(string Id)
        {
            try
            {
                var job = await _jobService.GetJobByEventIdAsync(Id);
                if (job == null)
                {
                    return NotFound($"Job with EventId {Id} not found");
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job with EventId: {EventId}", Id);
                return StatusCode(500, "An error occurred while retrieving the job");
            }
        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<JobResponse>> UpdateJob(string Id, [FromBody] UpdateJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedJob = await _jobService.UpdateJobAsync(Id, request);
                if (updatedJob == null)
                {
                    return NotFound($"Job with EventId {Id} not found");
                }

                _logger.LogInformation("Job updated successfully with EventId: {EventId}", Id);
                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job with EventId: {EventId}", Id);
                return StatusCode(500, "An error occurred while updating the job");
            }
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> CancelJob(string Id)
        {
            try
            {
                var cancelled = await _jobService.CancelJobAsync(Id);
                
                if (!cancelled)
                {
                    var job = await _jobService.GetJobByEventIdAsync(Id);
                    if (job == null)
                    {
                        return NotFound($"Job with EventId {Id} not found");
                    }
                    else
                    {
                        return BadRequest($"Job with EventId {Id} cannot be cancelled. It may already be executed or completed.");
                    }
                }

                _logger.LogInformation("Job cancelled successfully with EventId: {EventId}", Id);
                return Ok(new { success = true, message = $"Job with EventId {Id} has been cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling job with EventId: {EventId}", Id);
                return StatusCode(500, "An error occurred while cancelling the job");
            }
        }
    }
}
