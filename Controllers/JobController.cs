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

        [HttpGet("{eventId}")]
        public async Task<ActionResult<JobResponse>> GetJobByEventId(string eventId)
        {
            try
            {
                var job = await _jobService.GetJobByEventIdAsync(eventId);
                if (job == null)
                {
                    return NotFound($"Job with EventId {eventId} not found");
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job with EventId: {EventId}", eventId);
                return StatusCode(500, "An error occurred while retrieving the job");
            }
        }
    }
}
