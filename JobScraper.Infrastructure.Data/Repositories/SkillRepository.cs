using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly JobScraperDbContext _context;

    public SkillRepository(JobScraperDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        var entities = await _context.Skills.ToListAsync();
        return entities.Select(MapToModel);
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        var entity = await _context.Skills.FindAsync(id);
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<Skill?> GetByNameAsync(string name)
    {
        // 영문명 또는 한글명으로 검색
        var entity = await _context.Skills
            .FirstOrDefaultAsync(s => s.EnglishName == name || s.KoreanName == name);
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<Skill> CreateAsync(Skill skill)
    {
        var entity = MapToEntity(skill);
        _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        skill.Id = entity.Id;
        return skill;
    }

    public async Task<Skill> UpdateAsync(Skill skill)
    {
        var entity = await _context.Skills.FindAsync(skill.Id);
        if (entity == null)
            throw new ArgumentException("Skill not found", nameof(skill));

        entity.EnglishName = skill.EnglishName;
        entity.KoreanName = skill.KoreanName;
        entity.IconUrl = skill.IconUrl;
        await _context.SaveChangesAsync();
        return skill;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Skills.FindAsync(id);
        if (entity != null)
        {
            _context.Skills.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string name)
    {
        // 영문명 또는 한글명으로 존재 여부 확인
        return await _context.Skills.AnyAsync(s => s.EnglishName == name || s.KoreanName == name);
    }

    public async Task<IEnumerable<Skill>> GetSkillsByJobDetailIdAsync(int jobDetailId)
    {
        var entities = await _context.Skills
            .Where(s => s.JobDetails.Any(jd => jd.Id == jobDetailId))
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    private static Skill MapToModel(SkillEntity entity)
    {
        return new Skill
        {
            Id = entity.Id,
            EnglishName = entity.EnglishName,
            KoreanName = entity.KoreanName,
            IconUrl = entity.IconUrl
        };
    }

    private static SkillEntity MapToEntity(Skill model)
    {
        return new SkillEntity
        {
            Id = model.Id,
            EnglishName = model.EnglishName,
            KoreanName = model.KoreanName,
            IconUrl = model.IconUrl
        };
    }
}
