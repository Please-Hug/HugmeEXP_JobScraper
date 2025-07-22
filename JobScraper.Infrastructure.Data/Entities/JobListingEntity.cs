using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

/// <summary>
/// 채용공고 테이블 엔티티
/// </summary>
public class JobListingEntity
{
    [Key]
    public required int Id { get; set; }
    
    [MaxLength(100)]
    public string? SourceJobId { get; set; }
    
    [MaxLength(300)]
    public required string Title { get; set; }
    
    // Company 관계
    public required int CompanyId { get; set; }
    public required CompanyEntity Company { get; set; }
    
    public required string Url { get; set; }
    public required string Source { get; set; }
    
    public JobDetailEntity? JobDetail { get; set; }
}