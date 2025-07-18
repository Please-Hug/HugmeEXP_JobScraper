namespace JobScraper.Core.Models;

public class JobListing
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Company { get; set; }
    public required int Experience { get; set; } // 경력 요구사항 (0: 신입, 1 이상 : 경력 년수)
    public required string Url { get; set; }
    public required string Source { get; set; }
}