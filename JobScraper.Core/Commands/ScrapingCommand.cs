using JobScraper.Core.Enums;
using JobScraper.Core.Models;

namespace JobScraper.Core.Commands;

public class ScrapingCommand
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public CommandType Type { get; set; }
    public string? JobId { get; set; }
    public JobSearchParameters? SearchParameters { get; set; }
    public DateTime Timestamp { get; set; }
}