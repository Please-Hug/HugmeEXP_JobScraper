using JobScraper.Core.Commands;
using JobScraper.Core.Enums;
using JobScraper.Core.Models;
// ReSharper disable MemberCanBePrivate.Global

namespace JobScraper.Core;

    // 스크래핑 작업의 결과를 담는 클래스
    public class ScrapingResult
    {
        // 결과의 고유 ID
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // 이 결과를 생성한 명령의 ID
        public Guid CommandId { get; set; }
        
        // 명령 유형
        public CommandType CommandType { get; set; }
        
        // 데이터 소스
        public required string Source { get; set; }
        
        // 작업 성공 여부
        public bool Success { get; set; }
        
        // 실패한 경우 오류 메시지
        public string? ErrorMessage { get; set; }
        
        // 작업 시작 시간
        public DateTime StartTime { get; init; }
        
        // 작업 완료 시간
        public DateTime EndTime { get; set; }
        
        // 작업 소요 시간
        public long DurationMs => (long)(EndTime - StartTime).TotalMilliseconds;
        
        // 직업 목록 결과
        public List<JobListing> JobListings { get; set; } = [];
        
        // 직업 상세 정보 결과
        public JobDetail? JobDetail { get; set; }
        
        // 회사 정보 결과
        public Company? Company { get; set; }
        
        // 결과 메타데이터
        public Dictionary<string, string> Metadata { get; set; } = new();
        
        // 스크래핑 작업을 시작할 때 호출하는 헬퍼 메서드
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
        
        // 성공 결과를 생성하는 헬퍼 메서드
        public void SetSuccessListResult(IEnumerable<JobListing> listings, Dictionary<string, string> metadata)
        {
            Success = true;
            JobListings = new List<JobListing>(listings);
            EndTime = DateTime.UtcNow;
            
            Metadata = metadata;
        }
        
        // 성공 결과를 생성하는 헬퍼 메서드
        public void SetSuccessDetailResult(JobDetail detail)
        {
            Success = true;
            JobDetail = detail;
            EndTime = DateTime.UtcNow;
        }
        
        // 회사 정보 성공 결과를 생성하는 헬퍼 메서드
        public void SetSuccessCompanyResult(Company company)
        {
            Success = true;
            Company = company;
            EndTime = DateTime.UtcNow;
        }
        
        // 실패 결과를 생성하는 헬퍼 메서드
        public void SetFailure(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
            EndTime = DateTime.UtcNow;
        }
    }