using JobScraper.Core.Commands;
using JobScraper.Core.Enums;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Messaging.Clients;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapingController : ControllerBase
{
    private readonly IQueueClient _queueClient;
    private readonly ILogger<ScrapingController> _logger;
    private readonly IJobListingService _jobListingService;
    private readonly ICompanyService _companyService;

    public ScrapingController(IQueueClient queueClient, ILogger<ScrapingController> logger,
        IJobListingService jobListingService, ICompanyService companyService)
    {
        _queueClient = queueClient;
        _logger = logger;
        _jobListingService = jobListingService;
        _companyService = companyService;
    }

    [HttpPost("start-job-listings")]
    public async Task<ActionResult> StartJobListingsScraping([FromBody] StartScrapingRequest request)
    {
        if (string.IsNullOrEmpty(request.Source))
        {
            return BadRequest("Source is required");
        }

        var command = new ScrapingCommand
        {
            Id = Guid.NewGuid(),
            Source = request.Source,
            Type = CommandType.GetJobListings,
            SearchParameters = request.SearchParameters ?? new JobSearchParameters(),
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _queueClient.SendCommandAsync(command);
            _logger.LogInformation("Scraping command sent: {commandId} for source {source}", command.Id,
                command.Source);

            return Ok(new { CommandId = command.Id, Message = "Scraping started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send scraping command for source {source}", request.Source);
            return StatusCode(500, "Failed to start scraping");
        }
    }

    [HttpPost("start-job-detail")]
    public async Task<ActionResult> StartJobDetailScraping([FromBody] StartJobDetailScrapingRequest request)
    {
        if (string.IsNullOrEmpty(request.Source) || string.IsNullOrEmpty(request.JobId))
        {
            return BadRequest("Source and JobId are required");
        }

        var command = new ScrapingCommand
        {
            Id = Guid.NewGuid(),
            Source = request.Source,
            Type = CommandType.GetJobDetail,
            JobId = request.JobId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _queueClient.SendCommandAsync(command);
            _logger.LogInformation("Job detail scraping command sent: {commandId} for job {jobId}", command.Id,
                command.JobId);

            return Ok(new { CommandId = command.Id, Message = "Job detail scraping started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job detail scraping command for job {jobId}", request.JobId);
            return StatusCode(500, "Failed to start job detail scraping");
        }
    }

    [HttpPost("start-company")]
    public async Task<ActionResult> StartCompanyScraping([FromBody] StartCompanyScrapingRequest request)
    {
        if (string.IsNullOrEmpty(request.Source) || string.IsNullOrEmpty(request.CompanyId))
        {
            return BadRequest("Source and CompanyId are required");
        }

        var command = new ScrapingCommand
        {
            Id = Guid.NewGuid(),
            Source = request.Source,
            Type = CommandType.GetCompany,
            CompanyId = request.CompanyId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _queueClient.SendCommandAsync(command);
            _logger.LogInformation("Company scraping command sent: {commandId} for company {companyId}", command.Id,
                command.CompanyId);

            return Ok(new { CommandId = command.Id, Message = "Company scraping started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send company scraping command for company {companyId}", request.CompanyId);
            return StatusCode(500, "Failed to start company scraping");
        }
    }

    [HttpPost("start-bulk-scraping")]
    public async Task<ActionResult> StartBulkScraping([FromBody] BulkScrapingRequest request)
    {
        if (!request.Sources.Any())
        {
            return BadRequest("At least one source is required");
        }

        var commandIds = new List<Guid>();

        try
        {
            foreach (var source in request.Sources)
            {
                var command = new ScrapingCommand
                {
                    Id = Guid.NewGuid(),
                    Source = source,
                    Type = CommandType.GetJobListings,
                    SearchParameters = request.SearchParameters ?? new JobSearchParameters(),
                    Timestamp = DateTime.UtcNow
                };

                await _queueClient.SendCommandAsync(command);
                commandIds.Add(command.Id);
                _logger.LogInformation("Bulk scraping command sent: {commandId} for source {source}", command.Id,
                    command.Source);
            }

            return Ok(new
            {
                CommandIds = commandIds,
                Message = $"Bulk scraping started for {request.Sources.Count()} sources",
                TotalCommands = commandIds.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk scraping commands");
            return StatusCode(500, "Failed to start bulk scraping");
        }
    }

    [HttpPost("start-all-empty-job-details")]
    public async Task<ActionResult> GetAllJobDetails()
    {
        var emptyJobListings = await _jobListingService.GetAllJobListingsNotHavingDetailsAsync();
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
            await _queueClient.SendCommandAsync(command);
        }

        _logger.LogInformation("Started scraping job details for {count} job listings without details",
            emptyJobListings.Count());
        return Ok(new
        {
            Message = $"Started scraping job details for {emptyJobListings.Count()} job listings without details",
            TotalCommands = emptyJobListings.Count()
        });
    }
    
    [HttpPost("start-all-empty-company-details")]
    public async Task<ActionResult> GetAllCompanyDetails()
    {
        var emptyCompanies = await _companyService.GetAllCompaniesNotHavingDetailsAsync();
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
            await _queueClient.SendCommandAsync(command);
        }

        _logger.LogInformation("Started scraping company details for {count} companies without details",
            emptyCompanies.Count());
        return Ok(new
        {
            Message = $"Started scraping company details for {emptyCompanies.Count()} companies without details",
            TotalCommands = emptyCompanies.Count()
        });
    }
}

public class StartScrapingRequest
{
    public required string Source { get; set; }
    public JobSearchParameters? SearchParameters { get; set; }
}

public class StartJobDetailScrapingRequest
{
    public required string Source { get; set; }
    public required string JobId { get; set; }
}

public class StartCompanyScrapingRequest
{
    public required string Source { get; set; }
    public required string CompanyId { get; set; }
}

public class BulkScrapingRequest
{
    public required IEnumerable<string> Sources { get; set; }
    public JobSearchParameters? SearchParameters { get; set; }
}