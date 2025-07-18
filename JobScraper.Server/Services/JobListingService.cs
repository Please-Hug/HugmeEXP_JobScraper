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

    public async Task<JobListing> CreateJobListingAsync(JobListing jobListing)
    {
        return await _jobListingRepository.CreateAsync(jobListing);
    }

    public async Task<JobListing> UpdateJobListingAsync(JobListing jobListing)
    {
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
}
