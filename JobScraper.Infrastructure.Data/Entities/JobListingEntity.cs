using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class JobListingEntity
{
    [Key]
    public required int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Company { get; set; }
    
    public required int Experience { get; set; } // 경력 요구사항 (0: 신입, 1 이상 : 경력 년수)
    
    [Required]
    [MaxLength(1000)]
    public required string Url { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string Source { get; set; }
    
    public JobDetailEntity? JobDetail { get; set; }
}