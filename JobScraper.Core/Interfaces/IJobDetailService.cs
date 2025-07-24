using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface IJobDetailService
{
    Task<IEnumerable<JobDetail>> GetAllJobDetailsAsync();
    Task<JobDetail?> GetJobDetailByIdAsync(int id);
    Task<JobDetail> CreateJobDetailAsync(JobDetail jobDetail);
    Task<JobDetail> UpdateJobDetailAsync(JobDetail jobDetail);
    Task DeleteJobDetailAsync(int id);
    Task AddSkillToJobAsync(int jobDetailId, int skillId);
    Task RemoveSkillFromJobAsync(int jobDetailId, int skillId);
    Task<JobDetail> CreateJobDetailWithSkillsAsync(JobDetail jobDetail, IEnumerable<string> skillNames);
    Task<JobDetail?> GetJobDetailByJobListingId(int id);
}
