using HtmlAgilityPack.CssSelectors.NetCore;
using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Newtonsoft.Json.Linq;

namespace JobScraper.Bot.Scrapers;

public class WantedScraper : IJobScraper
{
    private readonly ILogger<WantedScraper> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WantedScraper(ILogger<WantedScraper> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<JobListing>> GetJobListingsAsync(JobSearchParameters parameters)
    {
        _logger.LogInformation("Getting Job Listings");
        var httpClient = _httpClientFactory.CreateClient("wanted");
        var sortBy = parameters.SortBy switch
        {
            "latest" => "job.latest_order",
            "recommend" => "job.recommend_order",
            "popular" => "job.popularity_order",
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
        
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WantedScraper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job listings: {response.ReasonPhrase}");
        }
        var content = await response.Content.ReadAsStringAsync();
        
        var json = JObject.Parse(content);
        
        var results = json["data"];
        if (results == null)
        {
            _logger.LogInformation("No job listings found in the response.");
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
                _logger.LogWarning(ex, "Error processing job listings");
            }
        }
        return jobListings;
    }

    public async Task<JobDetail> GetJobDetailAsync(string jobId)
    {
        var httpClient = _httpClientFactory.CreateClient("wanted");
        var id = jobId.Split("::").Last();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.wanted.co.kr/api/chaos/jobs/v4/{id}/details?{DateTime.Now.Ticks}=");
        var referer = $"https://www.wanted.co.kr/wd/{id}";
        SetupHeaders(request, referer);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WantedScraper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job details: {response.ReasonPhrase}");
        }
        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);
        var data = json["data"];

        var jobDetail = new JobDetail()
        {
            Benefits = data?["job"]?["detail"]?["benefits"]?.ToString() ?? string.Empty,
            Company = new Company()
            {
                ImageUrl = data?["job"]?["company"]?["logo_img"]?["origin"]?.ToString(),
                Name = data?["job"]?["company"]?["name"]?.ToString() ?? throw new InvalidOperationException(),
                SourceCompanyId = "wanted::" +
                                  (data["job"]?["company"]?["id"]?.ToString() ?? throw new InvalidOperationException()),
            },
            Description = data["job"]?["detail"]?["intro"]?.ToString() ?? string.Empty,
            Id = null,
            DueDate = data["job"]?["due_time"]?.ToObject<DateTime?>(),
            Education = 0, // 원티드에는 학력 정보가 없음
            Experience = data["job"]?["annual_from"]?.Value<int?>() ?? -1,
            Location = data["job"]?["address"]?["full_location"]?.ToString() ?? throw new InvalidOperationException(),
            LocationLatitude = (bool)data["job"]?["address"]?["geo_location"]?.HasValues ? data["job"]?["address"]?["geo_location"]?["location"]?["lat"]?.ToObject<decimal?>() : null,
            LocationLongitude = (bool)data["job"]?["address"]?["geo_location"]?.HasValues ? data["job"]?["address"]?["geo_location"]?["location"]?["lng"]?.ToObject<decimal?>() : null,
            MinSalary = 0,
            MaxSalary = 0,
            Source = "wanted",
            RequiredSkills = data["job"]?["skill_tags"]?.Select(s => new Skill()
            {
                Id = 0,
                Name = s["text"]?.ToString() ?? throw new InvalidOperationException(),
                IconUrl = null
            }).ToList() ?? [],
            Title = data["job"]?["detail"]?["position"]?.ToString() ?? throw new InvalidOperationException(),
            Url = $"https://www.wanted.co.kr/wd/{id}",
            SourceJobId = jobId,
            PreferredQualifications = data["job"]?["detail"]?["preferred_points"]?.ToString() ?? string.Empty,
            Requirements = data["job"]?["detail"]?["requirements"]?.ToString() ?? string.Empty,
            Tags = data["job"]?["attraction_tags"]?.Select(t => new Tag()
            {
                Name = t["title"]?.ToString() ?? string.Empty,
            }).ToList() ?? []
        };
        jobDetail.Tags = jobDetail.Tags
            .Where(t => !string.IsNullOrEmpty(t.Name))
            .DistinctBy(t => t.Name)
            .ToList();

        return jobDetail;
    }

    public async Task<Company> GetCompanyAsync(string companyId)
    {
        var httpClient = _httpClientFactory.CreateClient("wanted");
        var id = companyId.Split("::").Last();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.wanted.co.kr/company/{id}");
        var referer = $"https://www.wanted.co.kr/company/{id}";
        SetupHeaders(request, referer);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("WantedScraper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch company details: {response.ReasonPhrase}");
        }
        var content = await response.Content.ReadAsStringAsync();
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(content);
        var jsonString = doc.QuerySelector("#__NEXT_DATA__")?.InnerText;
        if (string.IsNullOrEmpty(jsonString))
        {
            _logger.LogError("No company data found in the response.");
            throw new InvalidOperationException("No company data found in the response.");
        }
        var json = JObject.Parse(jsonString);
        var data = json["props"]?["pageProps"]?["dehydrateState"]?["queries"]?[0]?["state"]?["data"];
        if (data == null)
        {
            _logger.LogError("No company data found in the response.");
            throw new InvalidOperationException("No company data found in the response.");
        }
        var foundedYear = data["foundedYear"]?.Value<int?>();
        var company = new Company()
        {
            Id = null,
            Name = data["name"]?.ToString() ?? throw new InvalidOperationException(),
            SourceCompanyId = "wanted::" + (data["wantedCompanyId"]?.ToString() ?? throw new InvalidOperationException()),
            Address = data["address"]?["full_location"]?.ToString(),
            ImageUrl = data["logo"]?.ToString(),
            Latitude = data["address"]?["geo_location"]?["location"]?["lat"]?.ToObject<decimal>(),
            Longitude = data["address"]?["geo_location"]?["location"]?["lng"]?.ToObject<decimal>(),
            EstablishedDate = foundedYear.HasValue 
                ? new DateTime(foundedYear.Value, 1, 1) 
                : null
        };
        
        return company;
    }
    
    private static void SetupHeaders(HttpRequestMessage request, string referer)
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