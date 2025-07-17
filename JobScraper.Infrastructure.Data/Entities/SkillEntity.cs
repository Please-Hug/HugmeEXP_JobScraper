using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class SkillEntity
{
    [Key]
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    // 다대다 관계를 위한 네비게이션 프로퍼티
    public ICollection<JobDetailEntity> JobDetails { get; set; } = new List<JobDetailEntity>();
}
