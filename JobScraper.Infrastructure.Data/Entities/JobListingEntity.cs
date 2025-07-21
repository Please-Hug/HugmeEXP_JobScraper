using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class JobListingEntity
{
    [Key]
    public required int Id { get; set; }
    public required string Title { get; set; }
    
    // Foreign key to Company
    public required int CompanyId { get; set; }
    public required CompanyEntity Company { get; set; }
    
    public required string Url { get; set; }
    public required string Source { get; set; }
    
    public JobDetailEntity? JobDetail { get; set; }
}