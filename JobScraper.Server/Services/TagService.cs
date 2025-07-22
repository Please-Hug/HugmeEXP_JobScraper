using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Server.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    
    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }
    
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _tagRepository.GetAllAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _tagRepository.GetByIdAsync(id);
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _tagRepository.GetByNameAsync(name);
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        if (await _tagRepository.GetByNameAsync(tag.Name) != null)
        {
            throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists");
        }

        return await _tagRepository.CreateAsync(tag);
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        if (!tag.Id.HasValue)
        {
            throw new ArgumentException("Tag ID is required for update operation.");
        }
        
        var existing = await _tagRepository.GetByIdAsync(tag.Id.Value);
        if (existing == null)
        {
            throw new ArgumentException($"Tag with ID {tag.Id} not found");
        }

        return await _tagRepository.UpdateAsync(tag);
    }

    public async Task DeleteAsync(int id)
    {
        await _tagRepository.DeleteAsync(id);
    }
}