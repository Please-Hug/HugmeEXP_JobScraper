namespace JobScraper.Core.Enums;

/// <summary>
/// 스크래핑 작업 타입
/// </summary>
public enum CommandType
{
    /// <summary>
    /// 채용공고 목록 스크래핑
    /// </summary>
    GetJobListings,
    
    /// <summary>
    /// 채용공고 상세정보 스크래핑
    /// </summary>
    GetJobDetail,
    
    /// <summary>
    /// 회사 정보 스크래핑
    /// </summary>
    GetCompany
}