using JobScraper.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace JobScraper.Server.Services;

public class KakaoMapService : IKakaoMapService
{
    private readonly ILogger<KakaoMapService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _kakaoApiKey;

    public KakaoMapService(ILogger<KakaoMapService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();
        _kakaoApiKey = configuration["KakaoApiKey"] ?? throw new InvalidOperationException("KakaoApiKey 설정이 누락되었습니다.");
    }
    
    public async Task<Tuple<decimal, decimal>> GetCoordinatesAsync(string address)
    {
        _logger.LogInformation("KakaoMapService: Getting coordinates for address: {address}", address);
        var httpClient = _httpClientFactory.CreateClient("KakaoMapService");
        
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"https://dapi.kakao.com/v2/local/search/address.json?analyze_type=similar&page=1&size=10&query={Uri.EscapeDataString(address)}");
        request.Headers.Add("Authorization", $"KakaoAK {_kakaoApiKey}");
        
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("KakaoMapService: Failed to get coordinates for address: {address}, StatusCode: {statusCode}", 
                address, response.StatusCode);
            throw new HttpRequestException($"Failed to get coordinates for address: {address}, StatusCode: {response.StatusCode}");
        }
        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("KakaoMapService: Successfully retrieved coordinates for address: {address}", address);
        var json = JObject.Parse(content);
        if (json["documents"] == null || !json["documents"]!.Any())
        {
            _logger.LogWarning("KakaoMapService: No coordinates found for address: {address}", address);
            throw new Exception($"No coordinates found for address: {address}");
        }
        var item = json["documents"]?.First;
        var x = item?["x"]!.Value<decimal>();
        var y = item?["y"]!.Value<decimal>();
        
        if (x == null || y == null)
        {
            _logger.LogWarning("KakaoMapService: Invalid coordinates for address: {address}", address);
            throw new Exception($"Invalid coordinates for address: {address}");
        }
        
        _logger.LogInformation("KakaoMapService: Coordinates for address {address} - X: {x}, Y: {y}", address, x, y);
        return new Tuple<decimal, decimal>(x.Value, y.Value);
    }
}