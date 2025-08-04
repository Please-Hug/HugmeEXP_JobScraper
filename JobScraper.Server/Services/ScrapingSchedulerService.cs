using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Messaging.Clients;
using JobScraper.Core.Commands;
using JobScraper.Core.Enums;

namespace JobScraper.Server.Services;

public class ScrapingSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScrapingSchedulerService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(60);

    public ScrapingSchedulerService(IServiceProvider serviceProvider, ILogger<ScrapingSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("스크래핑 스케줄러 서비스가 시작되었습니다. 60분마다 실행됩니다.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunScheduledScrapingAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "스케줄된 스크래핑 실행 중 오류가 발생했습니다.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunScheduledScrapingAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var queueClient = scope.ServiceProvider.GetRequiredService<IQueueClient>();
        var jobListingService = scope.ServiceProvider.GetRequiredService<IJobListingService>();
        var companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();

        _logger.LogInformation("스케줄된 스크래핑 작업을 시작합니다.");

        try
        {
            // 1. Wanted 채용 공고 스크래핑
            await StartJobListingsScrapingAsync(queueClient, "wanted");
            await Task.Delay(1000); // 1초 대기

            // 2. Jumpit 채용 공고 스크래핑
            await StartJobListingsScrapingAsync(queueClient, "jumpit");
            await Task.Delay(1000); // 1초 대기

            // 3. 모든 빈 회사 상세 정보 스크래핑
            await StartAllEmptyCompanyDetailsAsync(queueClient, companyService);
            await Task.Delay(1000); // 1초 대기

            // 4. 모든 빈 채용 상세 정보 스크래핑
            await StartAllEmptyJobDetailsAsync(queueClient, jobListingService);

            _logger.LogInformation("스케줄된 스크래핑 작업이 완료되었습니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "스케줄된 스크래핑 작업 실행 중 오류가 발생했습니다.");
        }
    }

    private async Task StartJobListingsScrapingAsync(IQueueClient queueClient, string source)
    {
        var searchParameters = new JobSearchParameters
        {
            Page = 0,
            PageSize = 20,
            Location = "all",
            MinExperienceYears = -1,
            SortBy = "latest"
        };

        var command = new ScrapingCommand
        {
            Id = Guid.NewGuid(),
            Source = source,
            Type = CommandType.GetJobListings,
            SearchParameters = searchParameters,
            Timestamp = DateTime.UtcNow
        };

        await queueClient.SendCommandAsync(command);
        _logger.LogInformation("채용 공고 스크래핑 명령을 전송했습니다: {source}, 명령 ID: {commandId}", source, command.Id);
    }

    private async Task StartAllEmptyCompanyDetailsAsync(IQueueClient queueClient, ICompanyService companyService)
    {
        var emptyCompanies = await companyService.GetAllCompaniesNotHavingDetailsAsync();
        var commands = new List<ScrapingCommand>();

        foreach (var company in emptyCompanies)
        {
            var command = new ScrapingCommand
            {
                Id = Guid.NewGuid(),
                Source = company.SourceCompanyId?.Split("::")[0] ?? "Unknown",
                Type = CommandType.GetCompany,
                CompanyId = company.SourceCompanyId,
                Timestamp = DateTime.UtcNow
            };
            commands.Add(command);
        }

        foreach (var command in commands)
        {
            await queueClient.SendCommandAsync(command);
        }

        _logger.LogInformation("빈 회사 상세 정보 스크래핑을 시작했습니다: {count}개 회사", emptyCompanies.Count());
    }

    private async Task StartAllEmptyJobDetailsAsync(IQueueClient queueClient, IJobListingService jobListingService)
    {
        var emptyJobListings = await jobListingService.GetAllJobListingsNotHavingDetailsAsync();
        var commands = new List<ScrapingCommand>();

        foreach (var jobListing in emptyJobListings)
        {
            var command = new ScrapingCommand
            {
                Id = Guid.NewGuid(),
                Source = jobListing.Source,
                Type = CommandType.GetJobDetail,
                JobId = jobListing.SourceJobId,
                Timestamp = DateTime.UtcNow
            };
            commands.Add(command);
        }

        foreach (var command in commands)
        {
            await queueClient.SendCommandAsync(command);
        }

        _logger.LogInformation("빈 채용 상세 정보 스크래핑을 시작했습니다: {count}개 채용 공고", emptyJobListings.Count());
    }
}
