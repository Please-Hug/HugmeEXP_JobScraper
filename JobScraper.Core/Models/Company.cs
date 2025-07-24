namespace JobScraper.Core.Models;

/// <summary>
/// 회사 정보
/// </summary>
public class Company
{
    /// <summary>
    /// 데이터베이스 Primary Key (자동 생성)
    /// </summary>
    public int? Id { get; set; }
    
    public required string Name { get; set; }
    
    /// <summary>
    /// 원본 사이트의 회사 고유 ID
    /// </summary>
    public string? SourceCompanyId { get; set; }
    
    public string? Address { get; set; }
    
    /// <summary>
    /// 회사 로고/이미지 URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? EstablishedDate { get; set; }
}
