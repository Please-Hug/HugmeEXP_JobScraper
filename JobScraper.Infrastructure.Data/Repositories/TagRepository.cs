using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

public class TagRepository : ITagRepository
{
    private readonly JobScraperDbContext _context;
    
    public TagRepository(JobScraperDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        var tags = await _context.Tags.ToListAsync();
        return tags.Select(MapToModel);
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        return tag != null ? MapToModel(tag) : null;
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == name);
        return tag != null ? MapToModel(tag) : null;
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        var entity = MapToEntity(tag);
        _context.Tags.Add(entity);
        await _context.SaveChangesAsync();
        tag.Id = entity.Id;
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        var entity = await _context.Tags.FindAsync(tag.Id);
        if (entity == null)
            throw new ArgumentException("Tag not found", nameof(tag));
        
        entity.Name = tag.Name;
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Tags.FindAsync(id);
        if (entity == null)
            return;
        
        _context.Tags.Remove(entity);
        await _context.SaveChangesAsync();
    }
    
    private static Tag MapToModel(TagEntity entity)
    {
        return new Tag
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    private static TagEntity MapToEntity(Tag model)
    {
        return new TagEntity
        {
            Id = model.Id ?? 0,
            Name = model.Name
        };
    }
}