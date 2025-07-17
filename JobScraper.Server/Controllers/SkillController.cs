using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetAllSkills()
    {
        var skills = await _skillService.GetAllSkillsAsync();
        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Skill>> GetSkill(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }
        return Ok(skill);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Skill>> GetSkillByName(string name)
    {
        var skill = await _skillService.GetSkillByNameAsync(name);
        if (skill == null)
        {
            return NotFound();
        }
        return Ok(skill);
    }

    [HttpGet("by-job/{jobDetailId}")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkillsByJobDetail(int jobDetailId)
    {
        var skills = await _skillService.GetSkillsByJobDetailIdAsync(jobDetailId);
        return Ok(skills);
    }

    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        try
        {
            var createdSkill = await _skillService.CreateSkillAsync(skill);
            return CreatedAtAction(nameof(GetSkill), new { id = createdSkill.Id }, createdSkill);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("get-or-create")]
    public async Task<ActionResult<Skill>> GetOrCreateSkill([FromBody] string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return BadRequest("Skill name is required");
        }

        var skill = await _skillService.GetOrCreateSkillAsync(skillName);
        return Ok(skill);
    }

    [HttpPost("get-or-create-batch")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetOrCreateSkills([FromBody] IEnumerable<string> skillNames)
    {
        if (!skillNames.Any())
        {
            return BadRequest("Skill names are required");
        }

        var skills = await _skillService.GetOrCreateSkillsAsync(skillNames);
        return Ok(skills);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Skill>> UpdateSkill(int id, [FromBody] Skill skill)
    {
        if (id != skill.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedSkill = await _skillService.UpdateSkillAsync(skill);
            return Ok(updatedSkill);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSkill(int id)
    {
        await _skillService.DeleteSkillAsync(id);
        return NoContent();
    }
}
