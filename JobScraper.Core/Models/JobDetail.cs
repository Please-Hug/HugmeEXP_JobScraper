namespace JobScraper.Core.Models;

public class JobDetail : JobListing
{
    public required string Description { get; set; }
    public required ICollection<Skill> RequiredSkills { get; set; } = new List<Skill>();
    public required long MinSalary { get; set; }
    public required long MaxSalary { get; set; }
    public required string Location { get; set; }
    public DateTime? DueDate { get; set; }  // 채용 마감일
    
    // 추가된 필드들
    public string? Education { get; set; }  // 학력
    public string? Experience { get; set; }  // 경력
    public string? Requirements { get; set; }  // 자격요건
    public string? PreferredQualifications { get; set; }  // 우대사항
    public string? Benefits { get; set; }  // 복지
    public decimal? LocationLatitude { get; set; }  // 위치 위도
    public decimal? LocationLongitude { get; set; }  // 위치 경도
}