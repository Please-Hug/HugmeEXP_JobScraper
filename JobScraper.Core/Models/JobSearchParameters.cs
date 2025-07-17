namespace JobScraper.Core.Models;

public class JobSearchParameters
{
    // 기본 검색 파라미터
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // 위치 관련
    public string Location { get; set; } = string.Empty;
    public bool Remote { get; set; } = false;
    
    // 직무 관련
    public List<string> JobCategories { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    
    // 경력 관련
    public int? MinExperienceYears { get; set; }
    public int? MaxExperienceYears { get; set; }
    public bool NewGraduate { get; set; } = false;
    
    // 고용 형태
    public List<string> EmploymentTypes { get; set; } = [];
    
    // 정렬 옵션
    public string SortBy { get; set; } = "latest";
}