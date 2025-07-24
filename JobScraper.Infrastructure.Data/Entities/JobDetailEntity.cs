using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    [MaxLength(30000)]
    public required string Description { get; set; }
    public ICollection<SkillEntity> RequiredSkills { get; set; } = new List<SkillEntity>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    [MaxLength(500)]
    public required string Location { get; set; }
    public DateTime? DueDate { get; set; }
    
    // 추가 상세 정보
    
    public int? Education { get; set; }
    public int? Experience { get; set; }
    [MaxLength(10000)]
    public string? Requirements { get; set; }
    [MaxLength(10000)]
    public string? PreferredQualifications { get; set; }
    [MaxLength(10000)]
    public string? Benefits { get; set; }
    [Column(TypeName = "decimal(10, 8)")]
    public decimal? LocationLatitude { get; set; }
    [Column(TypeName = "decimal(11, 8)")]
    public decimal? LocationLongitude { get; set; }
    
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    
    public required JobListingEntity JobListing { get; set; }
}