using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Newtonsoft.Json.Linq;

namespace JobScraper.Server.Services;

public class JobDetailService : IJobDetailService
{
    private readonly IJobDetailRepository _jobDetailRepository;
    private readonly ISkillService _skillService;
    private readonly IHttpClientFactory _httpClientFactory;

    public JobDetailService(IJobDetailRepository jobDetailRepository, ISkillService skillService, IHttpClientFactory httpClientFactory)
    {
        _jobDetailRepository = jobDetailRepository;
        _skillService = skillService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<JobDetail?> GetJobDetailByIdAsync(int id)
    {
        return await _jobDetailRepository.GetByIdAsync(id);
    }

    public async Task<JobDetail> CreateJobDetailAsync(JobDetail jobDetail)
    {
        jobDetail = await _jobDetailRepository.CreateAsync(jobDetail);
        await PushJobDetailToExternalServiceAsync((int)jobDetail.Id!);
        return jobDetail;
    }

    public async Task<JobDetail> UpdateJobDetailAsync(JobDetail jobDetail)
    {
        if (!jobDetail.Id.HasValue)
        {
            throw new ArgumentException("JobDetail ID is required for update operation.");
        }
        
        var existing = await _jobDetailRepository.GetByIdAsync(jobDetail.Id.Value);
        if (existing == null)
        {
            throw new ArgumentException($"JobDetail with ID {jobDetail.Id} not found.");
        }

        jobDetail = await _jobDetailRepository.UpdateAsync(jobDetail);
        await PushJobDetailToExternalServiceAsync((int)jobDetail.Id!);
        return jobDetail;
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

    public async Task<JobDetail?> GetJobDetailByJobListingId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("JobListing ID must be greater than zero.", nameof(id));
        }
        
        return await _jobDetailRepository.GetByJobListingIdAsync(id);
    }

    public Task<IEnumerable<JobDetail>> GetAllJobDetailsAsync()
    {
        return _jobDetailRepository.GetAllAsync();
    }
    
    private async Task PushJobDetailToExternalServiceAsync(int id)
    {
        var jobDetail = await _jobDetailRepository.GetByIdAsync(id);
        if (jobDetail == null)
        {
            throw new ArgumentException($"JobDetail with ID {id} not found.");
        }
        
        // 예시: 외부 서비스에 JobDetail을 푸시하는 로직
        var client = _httpClientFactory.CreateClient("ExternalService");
        if (client == null)
        {
            throw new InvalidOperationException("HTTP client for ExternalService is not configured.");
        }

        if (jobDetail.Company != null)
        {
            var json = new JObject
            {
                ["recruitmentSourceId"] = jobDetail.SourceJobId,
                ["title"] = jobDetail.Title,
                ["education"] = jobDetail.Education,
                ["experienceMin"] = jobDetail.Experience == -1 ? 0 : jobDetail.Experience,
                ["experienceMax"] = jobDetail.Experience == -1 ? 0 : jobDetail.Experience,
                ["qualification"] = jobDetail.Requirements ?? string.Empty,
                ["advantage"] = jobDetail.PreferredQualifications ?? string.Empty,
                ["welfare"] = jobDetail.Benefits ?? string.Empty,
                ["workLocation"] = jobDetail.Location,
                ["latitude"] = jobDetail.LocationLongitude.ToString(),
                ["longitude"] = jobDetail.LocationLatitude.ToString(),
                ["salaryMin"] = jobDetail.MinSalary,
                ["salaryMax"] = jobDetail.MaxSalary,
                ["link"] = jobDetail.Url,
                ["source"] = jobDetail.Source.ToUpper(),
                ["dueDate"] = $"{jobDetail.DueDate ?? DateTime.MaxValue:yyyy-MM-ddTHH:mm:ss}",
                ["company"] = new JObject
                {
                    ["companyName"] = jobDetail.Company.Name,
                    ["companyAddress"] = jobDetail.Company.Address,
                    ["latitude"] = jobDetail.Company.Latitude.ToString(),
                    ["longitude"] = jobDetail.Company.Longitude.ToString(),
                    ["establishmentDate"] = $"{jobDetail.Company.EstablishedDate ?? DateTime.MaxValue:yyyy-MM-dd}",
                    ["companyImageUrl"] = jobDetail.Company.ImageUrl,
                    ["companyDescription"] = string.Empty,
                    ["companySourceId"] = jobDetail.Company.SourceCompanyId ?? string.Empty
                },
                ["requiredSkills"] = new JArray(jobDetail.RequiredSkills.Select(skill => new JObject
                {
                    ["englishName"] = skill.Name,
                    ["koreanName"] = skill.Name,
                    ["iconUrl"] = skill.IconUrl ?? string.Empty
                })),
                ["tags"] = new JArray(jobDetail.Tags.Select(tag => new JObject
                {
                    ["tagName"] = tag.Name
                }))
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/api/v1/recruitments/scrape");
            request.Headers.Add("X-API-Key", "65f91852-3379-47a3-bcdb-b85241fc6b33");
            request.Content = new StringContent(json.ToString(), System.Text.Encoding.UTF8, "application/json");
        
            var response = await client.SendAsync(request);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to push JobDetail to external service. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }
    }
}
