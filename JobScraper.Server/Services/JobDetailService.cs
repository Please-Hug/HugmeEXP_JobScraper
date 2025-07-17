using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Server.Services;

public class JobDetailService : IJobDetailService
{
    private readonly IJobDetailRepository _jobDetailRepository;
    private readonly ISkillService _skillService;

    public JobDetailService(IJobDetailRepository jobDetailRepository, ISkillService skillService)
    {
        _jobDetailRepository = jobDetailRepository;
        _skillService = skillService;
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
        var existing = await _jobDetailRepository.GetByIdAsync(jobDetail.Id);
        if (existing == null)
        {
            throw new ArgumentException($"JobDetail with ID {jobDetail.Id} not found.");
        }

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
        // 스킬들을 먼저 생성하거나 조회
        var skills = await _skillService.GetOrCreateSkillsAsync(skillNames);
        
        // JobDetail에 스킬들 할당
        jobDetail.RequiredSkills = skills.ToList();
        
        // JobDetail 생성
        return await _jobDetailRepository.CreateAsync(jobDetail);
    }
}
