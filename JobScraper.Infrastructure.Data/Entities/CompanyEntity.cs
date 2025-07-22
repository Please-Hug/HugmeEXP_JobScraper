using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class CompanyEntity
{
    [Key]
    public required int Id { get; set; }
    
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(1000)]
    public string? ImageUrl { get; set; }  // 회사 이미지 URL
    
    public decimal? Latitude { get; set; }
    
    public decimal? Longitude { get; set; }
    
    public DateTime? EstablishedDate { get; set; }
    
    // Navigation property
    public ICollection<JobListingEntity> JobListings { get; set; } = new List<JobListingEntity>();
}
