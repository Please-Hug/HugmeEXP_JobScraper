using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Server.Services;

public class JobDetailService : IJobDetailService
{
    private readonly IJobDetailRepository _jobDetailRepository;
    private readonly ISkillRepository _skillRepository;

    public JobDetailService(IJobDetailRepository jobDetailRepository, ISkillRepository skillRepository)
    {
        _jobDetailRepository = jobDetailRepository;
        _skillRepository = skillRepository;
    }

    public async Task<IEnumerable<JobDetail>> GetAllJobDetailsAsync()
    {
        return await _jobDetailRepository.GetAllAsync();
    }

    public async Task<JobDetail?> GetJobDetailByIdAsync(int id)
    {
        return await _jobDetailRepository.GetByIdAsync(id);
    }

    public async Task<JobDetail> CreateJobDetailAsync(JobDetail jobDetail)
    {
        return await _jobDetailRepository.CreateAsync(jobDetail);
    }

    public async Task<JobDetail> UpdateJobDetailAsync(JobDetail jobDetail)
    {
        return await _jobDetailRepository.UpdateAsync(jobDetail);
    }

    public async Task DeleteJobDetailAsync(int id)
    {
        await _jobDetailRepository.DeleteAsync(id);
    }

    public async Task AddSkillToJobAsync(int jobDetailId, int skillId)
    {
        await _jobDetailRepository.AddSkillToJobAsync(jobDetailId, skillId);
    }

    public async Task RemoveSkillFromJobAsync(int jobDetailId, int skillId)
    {
        await _jobDetailRepository.RemoveSkillFromJobAsync(jobDetailId, skillId);
    }

    public async Task<JobDetail> CreateJobDetailWithSkillsAsync(JobDetail jobDetail, IEnumerable<string> skillNames)
    {
        // 스킬 이름들을 JobDetail의 RequiredSkills에 설정
        jobDetail.RequiredSkills = skillNames.ToList();
        
        // Repository에서 자동으로 스킬 엔티티를 생성/연결해줌
        return await _jobDetailRepository.CreateAsync(jobDetail);
    }
}
