namespace JobScraper.Core.Models;

public class JobDetail : JobListing
{
    public required string Description { get; set; }
    public required ICollection<Skill> RequiredSkills { get; set; } = new List<Skill>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    public required string Location { get; set; }
}