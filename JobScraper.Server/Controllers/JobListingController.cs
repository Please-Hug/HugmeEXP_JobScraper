using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobListingController : ControllerBase
{
    private readonly IJobListingService _jobListingService;

    public JobListingController(IJobListingService jobListingService)
    {
        _jobListingService = jobListingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobListing>>> GetAllJobListings()
    {
        var jobListings = await _jobListingService.GetAllJobListingsAsync();
        return Ok(jobListings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobListing>> GetJobListing(int id)
    {
        var jobListing = await _jobListingService.GetJobListingByIdAsync(id);
        if (jobListing == null)
        {
            return NotFound();
        }
        return Ok(jobListing);
    }

    [HttpGet("by-url")]
    public async Task<ActionResult<JobListing>> GetJobListingByUrl([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL is required");
        }

        var jobListing = await _jobListingService.GetJobListingByUrlAsync(url);
        if (jobListing == null)
        {
            return NotFound();
        }
        return Ok(jobListing);
    }

    [HttpGet("by-source/{source}")]
    public async Task<ActionResult<IEnumerable<JobListing>>> GetJobListingsBySource(string source)
    {
        var jobListings = await _jobListingService.GetJobListingsBySourceAsync(source);
        return Ok(jobListings);
    }

    [HttpPost]
    public async Task<ActionResult<JobListing>> CreateJobListing([FromBody] JobListing jobListing)
    {
        try
        {
            var createdJobListing = await _jobListingService.CreateJobListingAsync(jobListing);
            return CreatedAtAction(nameof(GetJobListing), new { id = createdJobListing.Id }, createdJobListing);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JobListing>> UpdateJobListing(int id, [FromBody] JobListing jobListing)
    {
        if (id != jobListing.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedJobListing = await _jobListingService.UpdateJobListingAsync(jobListing);
            return Ok(updatedJobListing);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJobListing(int id)
    {
        await _jobListingService.DeleteJobListingAsync(id);
        return NoContent();
    }

    [HttpGet("exists")]
    public async Task<ActionResult<bool>> CheckJobListingExists([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL is required");
        }

        var exists = await _jobListingService.JobListingExistsAsync(url);
        return Ok(exists);
    }
}
