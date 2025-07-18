using JobScraper.Core;
using JobScraper.Core.Enums;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultController : ControllerBase
{
    private readonly IJobListingService _jobListingService;
    private readonly IJobDetailService _jobDetailService;
    private readonly ISkillService _skillService;
    private readonly ILogger<ResultController> _logger;

    public ResultController(
        IJobListingService jobListingService,
        IJobDetailService jobDetailService,
        ISkillService skillService,
        ILogger<ResultController> logger)
    {
        _jobListingService = jobListingService;
        _jobDetailService = jobDetailService;
        _skillService = skillService;
        _logger = logger;
    }

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
                _logger.LogDebug("새 채용공고 저장: {title} - {company}", listing.Title, listing.Company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "채용공고 저장 실패: {title} - {company}", listing.Title, listing.Company);
                skippedCount++;
            }
        }

        _logger.LogInformation("채용공고 목록 처리 완료: 저장={processed}, 스킵={skipped}", processedCount, skippedCount);
    }

    private async Task ProcessJobDetailResult(ScrapingResult result)
    {
        if (result.JobDetail == null)
        {
            _logger.LogWarning("받은 채용 상세정보가 null임");
            return;
        }

        try
        {
            // 스킬 정보가 있다면 먼저 처리 - RequiredSkills는 이미 string 컬렉션
            if (result.JobDetail.RequiredSkills?.Any() == true)
            {
                // RequiredSkills는 이미 ICollection<string>이므로 직접 사용
                var skillNames = result.JobDetail.RequiredSkills;
                var processedSkills = await _skillService.GetOrCreateSkillsAsync(skillNames);
                
                // JobDetail 모델은 string 컬렉션을 사용하므로 스킬 이름만 저장
                result.JobDetail.RequiredSkills = processedSkills.Select(s => s.Name).ToList();
            }

            // JobDetail이 이미 존재하는지 확인
            var existingDetail = await _jobDetailService.GetJobDetailByIdAsync(result.JobDetail.Id);
            if (existingDetail != null)
            {
                // 기존 데이터 업데이트
                await _jobDetailService.UpdateJobDetailAsync(result.JobDetail);
                _logger.LogInformation("기존 채용 상세정보 업데이트: {id}", result.JobDetail.Id);
            }
            else
            {
                // 새 데이터 생성
                await _jobDetailService.CreateJobDetailAsync(result.JobDetail);
                _logger.LogInformation("새 채용 상세정보 저장: {id}", result.JobDetail.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "채용 상세정보 저장 실패: {id}", result.JobDetail.Id);
            throw;
        }
    }

    [HttpGet("health")]
    public ActionResult HealthCheck()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
