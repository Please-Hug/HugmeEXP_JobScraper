using JobScraper.Core;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Bot.Scrapers;

public class WantedScraper : IJobScraper
{
    public Task<IEnumerable<JobListing>> GetJobListingsAsync(JobSearchParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<JobDetail> GetJobDetailAsync(string jobId)
    {
        throw new NotImplementedException();
    }
}