using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobScraper.Core.Enums;

namespace JobScraper.Infrastructure.Data.Entities;

public class JobDetailEntity
{
    [Key]
    public int Id { get; set; }
    
    // JobListing과의 관계를 위한 Foreign Key
    public int JobListingId { get; set; }
    
    [Required]
    [MaxLength(10000)]
    public required string Description { get; set; }
    
    // RequiredSkills를 SkillEntity와의 다대다 관계로 변경
    public ICollection<SkillEntity> RequiredSkills { get; set; } = new List<SkillEntity>();
    
    public required long Salary { get; set; } // 연봉 (0: 협의)
    
    [Column(TypeName = "int")]
    public required EducationLevel EducationLevel { get; set; } // 최소 학력
    
    [Required]
    [MaxLength(500)]
    public required string Location { get; set; }
    
    // 추가 컬렉션 프로퍼티들 (JSON으로 저장)
    [Column(TypeName = "json")]
    public ICollection<string> Prefers { get; set; } = new List<string>(); // 우대사항
    
    [Column(TypeName = "json")]
    public ICollection<string> Tags { get; set; } = new List<string>(); // 태그
    
    [Column(TypeName = "json")]
    public ICollection<string> Qualifications { get; set; } = new List<string>(); // 자격요건
    
    public required JobListingEntity JobListing { get; set; }
}