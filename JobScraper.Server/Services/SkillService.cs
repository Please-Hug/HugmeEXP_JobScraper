using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using System.Linq;

namespace JobScraper.Server.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepository;

    public SkillService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
    {
        return await _skillRepository.GetAllAsync();
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        return await _skillRepository.GetByIdAsync(id);
    }

    public async Task<Skill?> GetSkillByNameAsync(string name)
    {
        return await _skillRepository.GetByNameAsync(name);
    }

    public async Task<Skill> CreateSkillAsync(Skill skill)
    {
        // 중복 영문명 또는 한글명 체크
        var existingByName = await _skillRepository.GetByNameAsync(skill.Name);
        
        if (existingByName != null)
        {
            throw new InvalidOperationException($"Skill with name '{skill.Name}' already exists.");
        }

        return await _skillRepository.CreateAsync(skill);
    }

    public async Task<Skill> UpdateSkillAsync(Skill skill)
    {
        var existing = await _skillRepository.GetByIdAsync(skill.Id);
        if (existing == null)
        {
            throw new ArgumentException($"Skill with ID {skill.Id} not found.");
        }

        return await _skillRepository.UpdateAsync(skill);
    }

    public async Task DeleteSkillAsync(int id)
    {
        await _skillRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Skill>> GetSkillsByJobDetailIdAsync(int jobDetailId)
    {
        return await _skillRepository.GetSkillsByJobDetailIdAsync(jobDetailId);
    }

    public async Task<Skill> GetOrCreateSkillAsync(string skillName)
    {
        var existing = await _skillRepository.GetByNameAsync(skillName);
        if (existing != null)
        {
            return existing;
        }

        // 스킬명이 영어인지 한국어인지 판단하여 적절한 필드에 할당
        var newSkill = new Skill
        {
            Id = 0, // 새로운 스킬이므로 0으로 설정
            Name = skillName
        };

        return await _skillRepository.CreateAsync(newSkill);
    }

    public async Task<IEnumerable<Skill>> GetOrCreateSkillsAsync(IEnumerable<string> skillNames)
    {
        var skills = new List<Skill>();
        
        foreach (var skillName in skillNames.Distinct())
        {
            var skill = await GetOrCreateSkillAsync(skillName);
            skills.Add(skill);
        }

        return skills;
    }
}
