using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface IJobListingRepository
{
    Task<IEnumerable<JobListing>> GetAllAsync();
    Task<JobListing?> GetByIdAsync(int id);
    Task<JobListing?> GetByUrlAsync(string url);
    Task<JobListing?> GetBySourceJobIdAsync(string sourceJobId);  // 새로 추가된 메서드
    Task<JobListing> CreateAsync(JobListing jobListing);
    Task<JobListing> UpdateAsync(JobListing jobListing);
    Task DeleteAsync(int id);
    Task<IEnumerable<JobListing>> GetBySourceAsync(string source);
    Task<bool> ExistsAsync(string url);
}
