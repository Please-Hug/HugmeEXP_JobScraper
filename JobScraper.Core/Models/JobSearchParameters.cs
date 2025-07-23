namespace JobScraper.Core.Models;

public class JobSearchParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // 위치 관련
    public string Location { get; set; } = string.Empty;
    
    // 경력 관련
    public int? MinExperienceYears { get; set; }
    // 정렬 옵션
    public string SortBy { get; set; } = "latest";
}