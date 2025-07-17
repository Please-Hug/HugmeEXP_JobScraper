using JobScraper.Bot.Scrapers;
using JobScraper.Core;
using JobScraper.Core.Commands;
using JobScraper.Core.Enums;
using JobScraper.Core.Interfaces;
using JobScraper.Infrastructure.Messaging.Clients;

namespace JobScraper.Bot.Services;

public class Worker(
    ILogger<Worker> logger,
    IQueueClient queueClient,
    WantedScraper wantedScraper,
    JumpitScraper jumpitScraper,
    IHttpClient httpClient)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("스크래퍼 봇이 시작됨: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var command = await queueClient.ReceiveCommandAsync();
                if (command != null)
                {
                    logger.LogInformation("새 명령 수신: {source}/{type}", command.Source, command.Type);
                    var result = await ProcessCommandAsync(command);
                    await httpClient.SendResultAsync(result);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "작업 처리 중 오류 발생");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<ScrapingResult> ProcessCommandAsync(ScrapingCommand command)
    {
        var result = ScrapingResult.StartNew(command);
        
        try
        {
            switch (command.Type)
            {
                case CommandType.GetJobListings:
                    if (command.SearchParameters == null)
                    {
                        throw new ArgumentException("SearchParameters is required for GetJobListings command");
                    }
                    
                    // 스크래퍼 선택
                    IJobScraper scraper = command.Source.ToLower() switch
                    {
                        "wanted" => wantedScraper,
                        "jumpit" => jumpitScraper,
                        _ => throw new ArgumentException($"Unknown source: {command.Source}")
                    };

                    // 스크래핑 실행
                    var listings = await scraper.GetJobListingsAsync(command.SearchParameters);
                
                    // 메타데이터 준비 (예: 총 개수, 페이지 정보 등)
                    var metadata = new Dictionary<string, string>
                    {
                        ["totalCount"] = listings.Count().ToString(),
                        ["page"] = command.SearchParameters.Page.ToString()
                    };
                
                    // 성공 결과 설정
                    result.SetSuccessListResult(listings, metadata);
                    break;

                case CommandType.GetJobDetail:
                    if (string.IsNullOrEmpty(command.JobId))
                    {
                        throw new ArgumentException("JobId is required for GetJobDetail command");
                    }
                    // 스크래퍼 선택 및 상세 정보 가져오기
                    IJobScraper detailScraper = command.Source.ToLower() switch
                    {
                        "wanted" => wantedScraper,
                        "jumpit" => jumpitScraper,
                        _ => throw new ArgumentException($"Unknown source: {command.Source}")
                    };
                
                    var detail = await detailScraper.GetJobDetailAsync(command.JobId);
                
                    // 성공 결과 설정
                    result.SetSuccessDetailResult(detail);
                    break;
                default:
                    throw new ArgumentException($"Unknown type: {command.Type}");
            }
        }
        catch (Exception ex)
        {
            // 실패 결과 설정
            logger.LogError(ex, "스크래핑 실패: {exMessage}", ex.Message);
            result.SetFailure(ex.Message);
        }
        
        logger.LogInformation("명령 처리 완료: {source}/{type}, 성공: {success}", 
            command.Source, command.Type, result.Success);
        return result;
    }
}
