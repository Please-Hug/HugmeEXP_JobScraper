namespace JobScraper.Core.Interfaces;

public interface IHttpClient
{
    Task SendResultAsync(ScrapingResult result);
}