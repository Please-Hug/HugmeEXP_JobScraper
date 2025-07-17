using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<Skill>> GetAllSkillsAsync();
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<Skill?> GetSkillByNameAsync(string name);
    Task<Skill> CreateSkillAsync(Skill skill);
    Task<Skill> UpdateSkillAsync(Skill skill);
    Task DeleteSkillAsync(int id);
    Task<IEnumerable<Skill>> GetSkillsByJobDetailIdAsync(int jobDetailId);
    Task<Skill> GetOrCreateSkillAsync(string skillName);
    Task<IEnumerable<Skill>> GetOrCreateSkillsAsync(IEnumerable<string> skillNames);
}
