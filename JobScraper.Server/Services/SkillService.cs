using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

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
        return await _skillRepository.CreateAsync(skill);
    }

    public async Task<Skill> UpdateSkillAsync(Skill skill)
    {
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
        var existingSkill = await _skillRepository.GetByNameAsync(skillName);
        if (existingSkill != null)
        {
            return existingSkill;
        }

        var newSkill = new Skill { Name = skillName };
        return await _skillRepository.CreateAsync(newSkill);
    }

    public async Task<IEnumerable<Skill>> GetOrCreateSkillsAsync(IEnumerable<string> skillNames)
    {
        var skills = new List<Skill>();
        
        foreach (var skillName in skillNames)
        {
            var skill = await GetOrCreateSkillAsync(skillName);
            skills.Add(skill);
        }

        return skills;
    }
}
