using JobScraper.Core.Commands;
using JobScraper.Core.Enums;
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

    public ScrapingController(IQueueClient queueClient, ILogger<ScrapingController> logger)
    {
        _queueClient = queueClient;
        _logger = logger;
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
            _logger.LogInformation("Scraping command sent: {commandId} for source {source}", command.Id, command.Source);
            
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
            _logger.LogInformation("Job detail scraping command sent: {commandId} for job {jobId}", command.Id, command.JobId);
            
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
            _logger.LogInformation("Company scraping command sent: {commandId} for company {companyId}", command.Id, command.CompanyId);
            
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
                _logger.LogInformation("Bulk scraping command sent: {commandId} for source {source}", command.Id, command.Source);
            }

            return Ok(new { 
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
