using JobScraper.Core;
using JobScraper.Core.Enums;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

/// <summary>
/// 스크래핑 결과 수신 및 처리 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ResultController : ControllerBase
{
    private readonly IJobListingService _jobListingService;
    private readonly IJobDetailService _jobDetailService;
    private readonly ICompanyService _companyService;
    private readonly ISkillService _skillService;
    private readonly ITagService _tagService;
    private readonly ILogger<ResultController> _logger;
    private readonly IKakaoMapService _kakaoMapService;

    public ResultController(
        IJobListingService jobListingService,
        IJobDetailService jobDetailService,
        ICompanyService companyService,
        ISkillService skillService,
        ITagService tagService,
        IKakaoMapService kakaoMapService,
        ILogger<ResultController> logger)
    {
        _jobListingService = jobListingService;
        _jobDetailService = jobDetailService;
        _companyService = companyService;
        _skillService = skillService;
        _tagService = tagService;
        _kakaoMapService = kakaoMapService;
        _logger = logger;
    }

    /// <summary>
    /// 봇으로부터 스크래핑 결과를 수신
    /// </summary>
    [HttpPost("scraping-result")]
    public async Task<ActionResult> ReceiveScrapingResult([FromBody] ScrapingResult result)
    {
        try
        {
            _logger.LogInformation("스크래핑 결과 수신: CommandId={commandId}, Source={source}, Type={type}, Success={success}",
                result.CommandId, result.Source, result.CommandType, result.Success);

            if (!result.Success)
            {
                _logger.LogWarning("스크래핑 실패 결과: {errorMessage}", result.ErrorMessage);
                return Ok(new { Message = "Failed scraping result received", CommandId = result.CommandId });
            }

            switch (result.CommandType)
            {
                case CommandType.GetJobListings:
                    await ProcessJobListingsResult(result);
                    break;

                case CommandType.GetJobDetail:
                    await ProcessJobDetailResult(result);
                    break;

                case CommandType.GetCompany:
                    await ProcessCompanyResult(result);
                    break;

                default:
                    _logger.LogWarning("알 수 없는 명령 타입: {commandType}", result.CommandType);
                    return BadRequest($"Unknown command type: {result.CommandType}");
            }

            _logger.LogInformation("스크래핑 결과 처리 완료: CommandId={commandId}", result.CommandId);
            return Ok(new { Message = "Scraping result processed successfully", CommandId = result.CommandId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "스크래핑 결과 처리 중 오류 발생: CommandId={commandId}", result.CommandId);
            return StatusCode(500, new { Message = "Error processing scraping result", Error = ex.Message });
        }
    }

    /// <summary>
    /// 채용공고 목록 스크래핑 결과 처리
    /// </summary>
    private async Task ProcessJobListingsResult(ScrapingResult result)
    {
        if (result.JobListings == null || !result.JobListings.Any())
        {
            _logger.LogInformation("받은 채용공고 목록이 비어있음");
            return;
        }

        var processedCount = 0;
        var skippedCount = 0;

        foreach (var listing in result.JobListings)
        {
            try
            {
                // 중복 확인
                var exists = await _jobListingService.JobListingExistsAsync(listing.Url);
                if (exists)
                {
                    skippedCount++;
                    _logger.LogDebug("이미 존재하는 채용공고 스킵: {url}", listing.Url);
                    continue;
                }

                // 새 채용공고 저장
                await _jobListingService.CreateJobListingAsync(listing);
                processedCount++;
                _logger.LogDebug("새 채용공고 저장: {title} - {company}", listing.Title, listing.Company.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "채용공고 저장 실패: {title} - {company}", listing.Title, listing.Company.Name);
                skippedCount++;
            }
        }

        _logger.LogInformation("채용공고 목록 처리 완료: 저장={processed}, 스킵={skipped}", processedCount, skippedCount);
    }

    /// <summary>
    /// 채용공고 상세정보 스크래핑 결과 처리
    /// </summary>
    private async Task ProcessJobDetailResult(ScrapingResult result)
    {
        if (result.JobDetail == null)
        {
            _logger.LogWarning("받은 채용 상세정보가 null임");
            return;
        }

        try
        {
            if (result.JobDetail.SourceJobId == null)
            {
                _logger.LogWarning("받은 채용 상세정보에 SourceJobId가 없음");
                return;
            }
            
            var sourceJobId = result.JobDetail.SourceJobId;

            if (string.IsNullOrEmpty(sourceJobId))
            {
                _logger.LogWarning("받은 채용 상세정보에 SourceJobId가 없음");
                return;
            }

            // SourceJobId로 기존 JobListing 찾기
            var existingJobListing = await _jobListingService.GetJobListingBySourceJobIdAsync(sourceJobId);
            if (existingJobListing == null)
            {
                _logger.LogWarning("SourceJobId {sourceJobId}에 해당하는 JobListing을 찾을 수 없음", sourceJobId);
                return;
            }

            // 기존 JobDetail이 있는지 확인
            var existingDetail = await _jobDetailService.GetJobDetailByJobListingId(existingJobListing.Id!.Value);

            if (!string.IsNullOrEmpty(result.JobDetail.Location) && result.JobDetail.LocationLongitude is null)
            {
                var coords = await _kakaoMapService.GetCoordinatesAsync(result.JobDetail.Location);
                result.JobDetail.LocationLongitude = coords.Item1;
                result.JobDetail.LocationLatitude = coords.Item2;
            }

            if (result.JobDetail.Company?.SourceCompanyId != null &&
                !string.IsNullOrEmpty(result.JobDetail.Company.Address) && result.JobDetail.Company.Longitude is null)
            {
                var coords = await _kakaoMapService.GetCoordinatesAsync(result.JobDetail.Company.Address);
                result.JobDetail.Company.Longitude = coords.Item1;
                result.JobDetail.Company.Latitude = coords.Item2;
                // 회사 정보 업데이트
                var existingCompany =
                    await _companyService.GetBySourceCompanyIdAsync(result.JobDetail.Company.SourceCompanyId);
                if (existingCompany != null)
                {
                    result.JobDetail.Company.Id = existingCompany.Id;
                    await _companyService.UpdateAsync(result.JobDetail.Company);
                    _logger.LogInformation("기존 회사 정보 업데이트: {name}", result.JobDetail.Company.Name);
                }
            }
            
            
            if (existingDetail != null)
            {
                result.JobDetail.Id = existingDetail.Id;
                // 기존 데이터 업데이트
                await _jobDetailService.UpdateJobDetailAsync(result.JobDetail);
                _logger.LogInformation("기존 채용 상세정보 업데이트: SourceJobId={sourceJobId}, DatabaseId={id}", sourceJobId, result.JobDetail.Id);
            }
            else
            {
                // 기존 JobDetail이 없으면 새 데이터 생성
                // 새 데이터 생성
                await _jobDetailService.CreateJobDetailAsync(result.JobDetail);
                _logger.LogInformation("새 채용 상세정보 저장: SourceJobId={sourceJobId}, DatabaseId={id}", sourceJobId, result.JobDetail.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "채용 상세정보 저장 실패: {jobDetailInfo}", 
                result.JobDetail.SourceJobId ?? result.JobDetail.Id?.ToString() ?? "Unknown");
            throw;
        }
    }

    /// <summary>
    /// 회사 정보 스크래핑 결과 처리
    /// </summary>
    private async Task ProcessCompanyResult(ScrapingResult result)
    {
        if (result.Company == null)
        {
            _logger.LogWarning("받은 회사 정보가 null임");
            return;
        }

        try
        {
            // 기존 회사 정보가 있는지 확인
            if (string.IsNullOrEmpty(result.Company.SourceCompanyId))
            {
                _logger.LogWarning("받은 회사 정보에 SourceCompanyId가 없음: {companyName}", result.Company.Name);
                return;
            }
            var existingCompany = await _companyService.GetBySourceCompanyIdAsync(result.Company.SourceCompanyId);
            if (existingCompany != null)
            {
                // 기존 회사 정보 업데이트 (새로운 정보로)
                var updatedCompany = await _companyService.GetOrCreateCompanyAsync(result.Company);
                _logger.LogInformation("기존 회사 정보 업데이트: {name}", result.Company.Name);
            }
            else
            {
                // 새 회사 정보 생성
                await _companyService.CreateAsync(result.Company);
                _logger.LogInformation("새 회사 정보 저장: {name}", result.Company.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 정보 저장 실패: {name}", result.Company.Name);
            throw;
        }
    }

    [HttpGet("health")]
    public ActionResult HealthCheck()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
