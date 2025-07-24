using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class TagEntity
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; }
    
    public ICollection<JobDetailEntity> JobDetails { get; set; } = new List<JobDetailEntity>();
}