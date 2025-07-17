using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface IJobScraper
{
    Task<IEnumerable<JobListing>> GetJobListingsAsync(JobSearchParameters parameters);
    Task<JobDetail> GetJobDetailAsync(string jobId);
}