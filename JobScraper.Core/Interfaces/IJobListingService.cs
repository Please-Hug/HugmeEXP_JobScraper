using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface IJobListingService
{
    Task<IEnumerable<JobListing>> GetAllJobListingsAsync();
    Task<JobListing?> GetJobListingByIdAsync(int id);
    Task<JobListing?> GetJobListingByUrlAsync(string url);
    Task<JobListing?> GetJobListingBySourceJobIdAsync(string sourceJobId);  // 새로 추가된 메서드
    Task<JobListing> CreateJobListingAsync(JobListing jobListing);
    Task<JobListing> UpdateJobListingAsync(JobListing jobListing);
    Task DeleteJobListingAsync(int id);
    Task<IEnumerable<JobListing>> GetJobListingsBySourceAsync(string source);
    Task<bool> JobListingExistsAsync(string url);
}
