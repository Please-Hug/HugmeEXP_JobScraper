namespace JobScraper.Core.Models;

/// <summary>
/// 채용공고 기본 정보
/// </summary>
public class JobListing
{
    /// <summary>
    /// 데이터베이스 Primary Key (자동 생성)
    /// </summary>
    public int? Id { get; set; }
    
    /// <summary>
    /// 원본 사이트의 채용공고 고유 ID
    /// </summary>
    public string? SourceJobId { get; set; }
    
    public required string Title { get; set; }
    public required Company Company { get; set; }
    public required string Url { get; set; }
    public required string Source { get; set; }
}