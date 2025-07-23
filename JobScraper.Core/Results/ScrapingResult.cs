using JobScraper.Core.Commands;
using JobScraper.Core.Enums;
using JobScraper.Core.Models;

namespace JobScraper.Core;

/// <summary>
/// 스크래핑 작업의 결과를 담는 클래스
/// </summary>
public class ScrapingResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommandId { get; set; }
    public CommandType CommandType { get; set; }
    public required string Source { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// 작업 소요 시간 (밀리초)
    /// </summary>
    public long DurationMs => (long)(EndTime - StartTime).TotalMilliseconds;
    
    public List<JobListing> JobListings { get; set; } = [];
    public JobDetail? JobDetail { get; set; }
    public Company? Company { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    /// <summary>
    /// 스크래핑 작업을 시작할 때 호출하는 헬퍼 메서드
    /// </summary>
    public static ScrapingResult StartNew(ScrapingCommand command)
    {
        return new ScrapingResult
        {
            CommandId = command.Id,
            CommandType = command.Type,
            Source = command.Source,
            StartTime = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// 채용공고 목록 스크래핑 성공 결과 설정
    /// </summary>
    public void SetSuccessListResult(IEnumerable<JobListing> listings, Dictionary<string, string> metadata)
    {
        Success = true;
        JobListings = new List<JobListing>(listings);
        EndTime = DateTime.UtcNow;
        Metadata = metadata;
    }
    
    /// <summary>
    /// 채용공고 상세정보 스크래핑 성공 결과 설정
    /// </summary>
    public void SetSuccessDetailResult(JobDetail detail)
    {
        Success = true;
        JobDetail = detail;
        EndTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 회사 정보 스크래핑 성공 결과 설정
    /// </summary>
    public void SetSuccessCompanyResult(Company company)
    {
        Success = true;
        Company = company;
        EndTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 스크래핑 실패 결과 설정
    /// </summary>
    public void SetFailure(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
        EndTime = DateTime.UtcNow;
    }
}
