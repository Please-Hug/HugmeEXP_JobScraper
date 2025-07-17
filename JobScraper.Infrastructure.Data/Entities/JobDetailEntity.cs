using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class JobDetailEntity
{
    [Key]
    public int Id { get; set; }
    
    // JobListing과의 관계를 위한 Foreign Key
    public int JobListingId { get; set; }
    
    public required string Description { get; set; }
    // RequiredSkills를 SkillEntity와의 다대다 관계로 변경
    public ICollection<SkillEntity> RequiredSkills { get; set; } = new List<SkillEntity>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    public required string Location { get; set; }
    
    public required JobListingEntity JobListing { get; set; }
}