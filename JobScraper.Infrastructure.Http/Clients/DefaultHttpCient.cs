using System.Text;
using System.Text.Json;
using JobScraper.Core;
using JobScraper.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace JobScraper.Infrastructure.Http.Clients;

public class DefaultHttpClient(ILogger<DefaultHttpClient> logger, IConfiguration configuration)
    : IHttpClient
{
    private readonly HttpClient _client = new();

    private readonly string _resultEndpoint = configuration["ResultEndpoint"] ??
                                              throw new InvalidOperationException("ResultEndpoint 설정이 없습니다");
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SendResultAsync(ScrapingResult result)
    {
        try
        {
            var json = JsonSerializer.Serialize(result, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_resultEndpoint, content);

            response.EnsureSuccessStatusCode();
            logger.LogInformation("결과 전송 성공: {commandId}", result.CommandId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "결과 전송 실패: {commandId}", result.CommandId);
            throw;
        }
    }
}