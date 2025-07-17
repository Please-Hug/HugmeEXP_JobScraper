namespace JobScraper.Core.Models;

public class JobListing
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Company { get; set; }
    public required string Url { get; set; }
    public required string Source { get; set; }
}