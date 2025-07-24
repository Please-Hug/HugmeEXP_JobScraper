using HtmlAgilityPack.CssSelectors.NetCore;
using JobScraper.Core;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Newtonsoft.Json.Linq;

namespace JobScraper.Bot.Scrapers;

public class JumpitScraper : IJobScraper
{
    private readonly ILogger<JumpitScraper> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public JumpitScraper(ILogger<JumpitScraper> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IEnumerable<JobListing>> GetJobListingsAsync(JobSearchParameters parameters)
    {
        _logger.LogInformation("Getting job listings");
        var httpClient = _httpClientFactory.CreateClient();
        var sortBy = parameters.SortBy switch
        {
            "latest" => "reg_dt",
            "popular" => "popular",
            _ => "reg_dt"
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://jumpit-api.saramin.co.kr/api/positions?sort={sortBy}&highlight=false&page={parameters.Page - 1}");

        const string referer = "https://jumpit.saramin.co.kr/";
        SetupHeaders(request, referer);
        
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("JumpitScrapper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job listings: {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        
        var json = JObject.Parse(content);

        var results = json["data"];
        
        var jobListings = new List<JobListing>();

        if (results == null) return jobListings;
        
        foreach (var item in results)
        {
            try // 일부가 실패하더라도 전체를 처리하기 위해 try-catch 사용
            {
                var jobListing = new JobListing
                {
                    Id = null,
                    Source = "jumpit",
                    Company = new Company
                    {
                        Id = null,
                        Name = item["companyName"]?.ToString() ?? throw new InvalidOperationException("Company name not found"),
                        SourceCompanyId = "jumpit::" + 
                                           (item["encodedSerialNumber"]?.ToString() ?? throw new InvalidOperationException("Company ID not found")),
                    },
                    SourceJobId = "jumpit::" +
                                  (item["id"] ?? throw new InvalidOperationException("Job ID not found")),
                    Title = item["title"]?.ToString() ?? throw new InvalidOperationException("Job title not found"),
                    Url = $"https://jumpit.saramin.co.kr/position/{item["id"]}"
                };

                jobListings.Add(jobListing);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing job listings");
            }
        }

        return jobListings;
    }

    public async Task<JobDetail> GetJobDetailAsync(string jobId)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var id = jobId.Split("::").Last();
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://jumpit.saramin.co.kr/position/{id}");
        const string referer = "https://jumpit.saramin.co.kr/";
        
        SetupHeaders(request, referer);
        
        var response = await httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("JumpitScrapper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job detail: {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        
        
        
        
        return null;
    }

    public Task<Company> GetCompanyAsync(string companyId)
    {
        // TODO: 실제 점핏 회사 정보 크롤링 구현
        throw new NotImplementedException();
    }
    
    private static void SetupHeaders(HttpRequestMessage request, string referer)
    {
        request.Headers.Clear();
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Whale/4.32.315.22 Safari/537.36");
        request.Headers.Add("Accept", "application/json, text/plain");
        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Whale\";v=\"4\", \"Not.A/Brand\";v=\"99\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("Origin", "https://jumpit.saramin.co.kr");
        request.Headers.Add("Sec-Fetch-Site", "same-site");
        request.Headers.Add("Sec-Fetch-Mode", "cors");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Referer", referer);
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
    }
}