using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobDetailController : ControllerBase
{
    private readonly IJobDetailService _jobDetailService;

    public JobDetailController(IJobDetailService jobDetailService)
    {
        _jobDetailService = jobDetailService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDetail>> GetJobDetail(int id)
    {
        var jobDetail = await _jobDetailService.GetJobDetailByIdAsync(id);
        if (jobDetail == null)
        {
            return NotFound();
        }
        return Ok(jobDetail);
    }

    [HttpPost]
    public async Task<ActionResult<JobDetail>> CreateJobDetail([FromBody] JobDetail jobDetail)
    {
        var createdJobDetail = await _jobDetailService.CreateJobDetailAsync(jobDetail);
        return CreatedAtAction(nameof(GetJobDetail), new { id = createdJobDetail.Id }, createdJobDetail);
    }

    [HttpPost("with-skills")]
    public async Task<ActionResult<JobDetail>> CreateJobDetailWithSkills([FromBody] CreateJobDetailWithSkillsRequest request)
    {
        if (request.JobDetail == null || !request.SkillNames.Any())
        {
            return BadRequest("JobDetail and skill names are required");
        }

        var createdJobDetail = await _jobDetailService.CreateJobDetailWithSkillsAsync(request.JobDetail, request.SkillNames);
        return CreatedAtAction(nameof(GetJobDetail), new { id = createdJobDetail.Id }, createdJobDetail);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JobDetail>> UpdateJobDetail(int id, [FromBody] JobDetail jobDetail)
    {
        if (id != jobDetail.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedJobDetail = await _jobDetailService.UpdateJobDetailAsync(jobDetail);
            return Ok(updatedJobDetail);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJobDetail(int id)
    {
        await _jobDetailService.DeleteJobDetailAsync(id);
        return NoContent();
    }

    [HttpPost("{jobDetailId}/skills/{skillId}")]
    public async Task<ActionResult> AddSkillToJob(int jobDetailId, int skillId)
    {
        await _jobDetailService.AddSkillToJobAsync(jobDetailId, skillId);
        return NoContent();
    }

    [HttpDelete("{jobDetailId}/skills/{skillId}")]
    public async Task<ActionResult> RemoveSkillFromJob(int jobDetailId, int skillId)
    {
        await _jobDetailService.RemoveSkillFromJobAsync(jobDetailId, skillId);
        return NoContent();
    }
}

public class CreateJobDetailWithSkillsRequest
{
    public required JobDetail JobDetail { get; set; }
    public required IEnumerable<string> SkillNames { get; set; }
}
