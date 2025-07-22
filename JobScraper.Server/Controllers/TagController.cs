using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Skill>> GetTag(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag == null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Tag>> GetTagByName(string name)
    {
        var tag = await _tagService.GetByNameAsync(name);
        if (tag == null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag([FromBody] Tag tag)
    {
        try
        {
            var createdTag = await _tagService.CreateAsync(tag);
            return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(int id)
    {
        await _tagService.DeleteAsync(id);
        return NoContent();
    }
}