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
    
    [MaxLength(1000)]
    public required string Url { get; set; }
    [MaxLength(50)] // 추후 열거형으로 변경 가능
    public required string Source { get; set; }
    
    public JobDetailEntity? JobDetail { get; set; }
}