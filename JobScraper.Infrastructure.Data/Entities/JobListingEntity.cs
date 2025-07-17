using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class JobListingEntity
{
    [Key]
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Company { get; set; }
    public required string Url { get; set; }
    public required string Source { get; set; }
    
    public JobDetailEntity? JobDetail { get; set; }
}