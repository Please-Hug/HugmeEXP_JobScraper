using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Server.Services;

public class JobListingService : IJobListingService
{
    private readonly IJobListingRepository _jobListingRepository;

    public JobListingService(IJobListingRepository jobListingRepository)
    {
        _jobListingRepository = jobListingRepository;
    }

    public async Task<IEnumerable<JobListing>> GetAllJobListingsAsync()
    {
        return await _jobListingRepository.GetAllAsync();
    }

    public async Task<JobListing?> GetJobListingByIdAsync(int id)
    {
        return await _jobListingRepository.GetByIdAsync(id);
    }

    public async Task<JobListing?> GetJobListingByUrlAsync(string url)
    {
        return await _jobListingRepository.GetByUrlAsync(url);
    }

    public async Task<JobListing?> GetJobListingBySourceJobIdAsync(string sourceJobId)
    {
        return await _jobListingRepository.GetBySourceJobIdAsync(sourceJobId);
    }

    public async Task<JobListing> CreateJobListingAsync(JobListing jobListing)
    {
        // 중복 URL 체크
        var existing = await _jobListingRepository.GetByUrlAsync(jobListing.Url);
        if (existing != null)
        {
            throw new InvalidOperationException($"Job listing with URL '{jobListing.Url}' already exists.");
        }

        return await _jobListingRepository.CreateAsync(jobListing);
    }

    public async Task<JobListing> UpdateJobListingAsync(JobListing jobListing)
    {
        if (jobListing.Id == null)
        {
            throw new ArgumentException("Job listing ID is required for update operation.");
        }
        
        var existing = await _jobListingRepository.GetByIdAsync(jobListing.Id.Value);
        if (existing == null)
        {
            throw new ArgumentException($"Job listing with ID {jobListing.Id} not found.");
        }

        return await _jobListingRepository.UpdateAsync(jobListing);
    }

    public async Task DeleteJobListingAsync(int id)
    {
        await _jobListingRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<JobListing>> GetJobListingsBySourceAsync(string source)
    {
        return await _jobListingRepository.GetBySourceAsync(source);
    }

    public async Task<bool> JobListingExistsAsync(string url)
    {
        return await _jobListingRepository.ExistsAsync(url);
    }

    public Task<IEnumerable<JobListing>> GetAllJobListingsNotHavingDetailsAsync()
    {
        return _jobListingRepository.GetAllNotHavingDetailsAsync();
    }
}
