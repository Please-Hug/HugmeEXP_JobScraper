using JobScraper.Core.Enums;
using JobScraper.Core.Models;

namespace JobScraper.Core.Commands;

/// <summary>
/// 스크래핑 작업 명령
/// </summary>
public class ScrapingCommand
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public CommandType Type { get; set; }
    
    /// <summary>
    /// 채용공고 상세정보 스크래핑 시 필요한 Job ID
    /// </summary>
    public string? JobId { get; set; }
    
    /// <summary>
    /// 회사 정보 스크래핑 시 필요한 Company ID
    /// </summary>
    public string? CompanyId { get; set; }
    
    /// <summary>
    /// 채용공고 목록 스크래핑 시 사용하는 검색 조건
    /// </summary>
    public JobSearchParameters? SearchParameters { get; set; }
    
    public DateTime Timestamp { get; set; }
}