using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class CompanyEntity
{
    [Key]
    public required int Id { get; set; }
    
    public required string Name { get; set; }
    
    public string? Address { get; set; }
    
    public decimal? Latitude { get; set; }
    
    public decimal? Longitude { get; set; }
    
    public DateTime? EstablishedDate { get; set; }
    
    // Navigation property
    public ICollection<JobListingEntity> JobListings { get; set; } = new List<JobListingEntity>();
}
