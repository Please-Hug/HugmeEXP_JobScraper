namespace JobScraper.Core.Models;

/// <summary>
/// 채용공고 상세 정보 (JobListing 상속)
/// </summary>
public class JobDetail : JobListing
{
    public required string Description { get; set; }
    public required ICollection<Skill> RequiredSkills { get; set; } = new List<Skill>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    public required string Location { get; set; }
    
    /// <summary>
    /// 채용 마감일
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    // 추가 상세 정보
    public int? Education { get; set; }
    public int? Experience { get; set; }
    public string? Requirements { get; set; }
    public string? PreferredQualifications { get; set; }
    public string? Benefits { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}