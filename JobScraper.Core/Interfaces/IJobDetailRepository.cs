using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface IJobDetailRepository
{
    Task<JobDetail?> GetByIdAsync(int id);
    Task<JobDetail> CreateAsync(JobDetail jobDetail);
    Task<JobDetail> UpdateAsync(JobDetail jobDetail);
    Task DeleteAsync(int id);
    Task AddSkillToJobAsync(int jobDetailId, int skillId);
    Task RemoveSkillFromJobAsync(int jobDetailId, int skillId);
    Task<JobDetail?> GetByJobListingIdAsync(int id);
}
