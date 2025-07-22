using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

/// <summary>
/// 채용공고 상세 정보 테이블 엔티티
/// </summary>
public class JobDetailEntity
{
    [Key]
    public int Id { get; set; }
    
    // JobListing과의 관계
    public int JobListingId { get; set; }
    
    public required string Description { get; set; }
    public ICollection<SkillEntity> RequiredSkills { get; set; } = new List<SkillEntity>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    public required string Location { get; set; }
    public DateTime? DueDate { get; set; }
    
    // 추가 상세 정보
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? Requirements { get; set; }
    public string? PreferredQualifications { get; set; }
    public string? Benefits { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    
    public required JobListingEntity JobListing { get; set; }
}