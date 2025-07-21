using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface ISkillRepository
{
    Task<IEnumerable<Skill>> GetAllAsync();
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill?> GetByNameAsync(string name);
    Task<Skill> CreateAsync(Skill skill);
    Task<Skill> UpdateAsync(Skill skill);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string name);
    Task<IEnumerable<Skill>> GetSkillsByJobDetailIdAsync(int jobDetailId);
}
