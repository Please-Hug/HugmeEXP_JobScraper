using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Newtonsoft.Json.Linq;

namespace JobScraper.Bot.Scrapers;

public class WantedScraper(ILogger<WantedScraper> logger) : IJobScraper
{
    private readonly HttpClient _httpClient = new HttpClient();
    public async Task<IEnumerable<JobListing>> GetJobListingsAsync(JobSearchParameters parameters)
    {
        logger.LogInformation("Getting Job Listings");
        var sortBy = parameters.SortBy switch
        {
            "latest" => "job.latest_order",
            "recommend" => "job.recommend_order",
            "popularity_order" => "job.popularity_order",
            _ => "job.latest_order"
        };
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"https://www.wanted.co.kr/api/chaos/navigation/v1/results?" +
            $"{DateTime.Now.Ticks}=" +
            $"&job_group_id=518" +
            $"&country=kr&job_sort={sortBy}" +
            $"&years={parameters.MinExperienceYears}" +
            $"&locations={parameters.Location}" +
            $"&limit={parameters.PageSize}" +
            $"&offset={parameters.Page * parameters.PageSize}");
        
        var referer = $"https://www.wanted.co.kr/wdlist/518?country=kr&job_sort={sortBy}" +
                      $"&years={parameters.MinExperienceYears}" +
                      $"&locations={parameters.Location}";
        SetupHeaders(request, referer);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("WantedScraper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job listings: {response.ReasonPhrase}");
        }
        var content = await response.Content.ReadAsStringAsync();
        
        var json = JObject.Parse(content);
        
        var results = json["data"];
        if (results == null)
        {
            logger.LogInformation("No job listings found in the response.");
            throw new InvalidOperationException("No job listings found in the response.");
        }
        
        var jobListings = new List<JobListing>();
        
        foreach (var item in results)
        {
            try // 일부가 실패하더라도 전체를 처리하기 위해 try-catch 사용
            {
                var jobListing = new JobListing()
                {
                    Id = null,
                    Source = "wanted",
                    Company = new Company()
                    {
                        Id = null,
                        Name = item["company"]?["name"]?.ToString() ?? throw new InvalidOperationException(),
                        SourceCompanyId = "wanted::" +
                                          (item["company"]?["id"]?.ToString() ?? throw new InvalidOperationException()),
                    },
                    SourceJobId = "wanted::" + (item["id"]?.ToString() ?? throw new InvalidOperationException()),
                    Title = item["position"]?.ToString() ?? throw new InvalidOperationException(),
                    Url =
                        $"https://www.wanted.co.kr/wd/{item["id"]?.ToString() ?? throw new InvalidOperationException()}"
                };
                jobListings.Add(jobListing);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error processing job listings");
            }
        }
        
        return jobListings;
    }

    public Task<JobDetail> GetJobDetailAsync(string jobId)
    {
        throw new NotImplementedException();
    }

    public Task<Company> GetCompanyAsync(string companyId)
    {
        // TODO: 실제 원티드 회사 정보 크롤링 구현
        throw new NotImplementedException();
    }
    
    private void SetupHeaders(HttpRequestMessage request, string referer)
    {
        request.Headers.Clear();
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("WANTED-User-Agent", "user-web");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Whale\";v=\"4\", \"Not.A/Brand\";v=\"99\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("WANTED-User-Language", "ko");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Whale/4.32.315.22 Safari/537.36");
        request.Headers.Add("Accept", "application/json, text/plain");
        request.Headers.Add("WANTED-User-Country", "KR");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-Mode", "cors");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Referer", referer);
        request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
    }
}