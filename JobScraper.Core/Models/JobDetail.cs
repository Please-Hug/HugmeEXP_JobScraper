using JobScraper.Core.Enums;

namespace JobScraper.Core.Models;

public class JobDetail : JobListing
{
    public required string Description { get; set; }
    public required ICollection<string> RequiredSkills { get; set; } = new List<string>(); // 요구 스킬
    public required long Salary { get; set; } // 최소 연봉 (0: 협의)
    public required EducationLevel EducationLevel { get; set; } // 최소 학력
    public required ICollection<string> Prefers { get; set; } = new List<string>(); // 우대사항
    public required ICollection<string> Tags { get; set; } = new List<string>(); // 태그
    public required ICollection<string> Qualifications { get; set; } = new List<string>(); // 자격요건
    public required string Location { get; set; } // 근무지 주소
}