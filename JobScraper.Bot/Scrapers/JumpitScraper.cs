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
    
    /*
    { "label": "무관", "value": 0 },
    { "label": "고졸", "value": 10 },
    { "label": "초대졸", "value": 20 },
    { "label": "대졸", "value": 30 },
    { "label": "석사", "value": 40 },
    { "label": "박사", "value": 50 }
     */
    private static readonly Dictionary<string, int> EducationMap = new()
    {
        { "무관", 0 },
        {"고등학교졸업 이상", 10},
        {"고등학교졸업 이상(졸업예정자 가능)", 10},
        { "대학졸업(2,3년) 이상", 20 },
        { "대학졸업(2,3년) 이상(졸업예정자 가능)", 20 },
        { "대학교졸업(4년) 이상", 30 },
        { "대학교졸업(4년) 이상(졸업예정자 가능)", 30 },
        { "석사졸업 이상", 40 },
        { "석사졸업 이상(졸업예정자 가능)", 40 },
        { "박사졸업", 50 },
        { "박사졸업(졸업예정자 가능)", 50 },
        { "박사졸업 이상", 50 },
        { "박사졸업 이상(졸업예정자 가능)", 50 }, // 이상이 있는 경우가 있는지 모르겠지만 일단 추가 
    };

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
            $"https://jumpit-api.saramin.co.kr/api/positions?sort={sortBy}&highlight=false&page={parameters.Page + 1}");

        const string referer = "https://jumpit.saramin.co.kr/";
        SetupListingHeaders(request, referer);
        
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("JumpitScrapper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job listings: {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        
        var json = JObject.Parse(content);

        var results = json["result"];
        
        var jobListings = new List<JobListing>();

        var positions = results?["positions"];
        if (positions == null || !positions.Any())
        {
            _logger.LogWarning("No job listings found");
            return jobListings;
        }
        
        foreach (var item in positions)
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
                        Name = item["companyName"]?.ToString() ??
                               throw new InvalidOperationException("Company name not found"),
                        SourceCompanyId = "jumpit::" +
                                          (item["encodedSerialNumber"]?.ToString() ??
                                           throw new InvalidOperationException("Company ID not found")),
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
        const string referer = "https://jumpit.saramin.co.kr/positions?sort=popular";
        
        SetupDetailHeaders(request, referer);
        
        var response = await httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("JumpitScrapper returned error code {ResponseStatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch job detail: {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();

        content = content.Split('\n').Where(line => line.Contains("serviceInfo"))
            .Select(line => line.Substring(line.IndexOf(':') + 1)).FirstOrDefault();

        if (content == null)
            throw new HttpRequestException($"Failed to fetch job detail: {response.ReasonPhrase}");
        var json = JArray.Parse(content).SkipWhile(x => x.Type != JTokenType.Object).FirstOrDefault();
        var data = json?["state"]?["queries"]?[0]?["state"]?["data"];
        if (data == null)
        {
            _logger.LogWarning("Job detail not found for jobId: {JobId}", jobId);
            throw new HttpRequestException($"Failed to fetch job detail: {response.ReasonPhrase}");
        }

        var jobDetail = new JobDetail()
        {
            Benefits = data["welfares"]?.ToString(),
            Company = new Company()
            {
                Name = data["companyName"]?.ToString() ?? throw new InvalidOperationException("Company name not found"),
                SourceCompanyId = "jumpit::" +
                                  (data["encodedSerialNumber"]?.ToString() ??
                                   throw new InvalidOperationException("Company ID not found")),
                ImageUrl = data["logo"]?.ToString() ?? throw new InvalidOperationException("Image URL not found"),
                EstablishedDate = DateTime.Now.AddYears(data["establishPeriod"]?.Value<int>() - 1 ?? 0),
                Address = data["location"]?.ToString() ?? string.Empty,
            },
            Description = (data["serviceInfo"]?.ToString() ?? string.Empty)
            + (data["responsibility"]?.ToString() ?? string.Empty),
            Id = null,
            DueDate = data["closedAt"]?.ToObject<DateTime?>(),
            Education = EducationMap[data["educationName"]?.ToString() ?? "무관"], // 기본값을 무관으로 채움
            Experience = data["minCareer"]?.Value<int>(),
            Location = data["workingPlaces"]?[0]?["address"]?.ToString() ?? throw new InvalidOperationException("Location not found"),
            LocationLatitude = null,
            LocationLongitude = null,
            MinSalary = 0,
            MaxSalary = 0,
            Source = "jumpit",
            RequiredSkills = data["techStacks"]?.Select(s => new Skill()
            {
                Id = 0,
                Name = s["stack"]?.ToString() ?? throw new InvalidOperationException("Skill name not found"),
                IconUrl = s["imagePath"]?.ToString() ?? null,
            }).ToList() ?? [],
            Title = data["title"]?.ToString() ?? throw new InvalidOperationException("Title not found"),
            Url = $"https://jumpit.saramin.co.kr/position/{id}",
            SourceJobId = jobId,
            PreferredQualifications = data["preferredRequirements"]?.ToString() ?? string.Empty,
            Requirements = data["qualifications"]?.ToString() ?? string.Empty,
            Tags = data["tags"]?.Select(t => new Tag()
            {
                Name = t["name"]?.ToString() ?? string.Empty,
            }).ToList() ?? [],
        };
        jobDetail.Tags = jobDetail.Tags
            .Where(t => !string.IsNullOrEmpty(t.Name))
            .DistinctBy(t => t.Name)
            .ToList();

        return jobDetail;
    }

    public Task<Company> GetCompanyAsync(string companyId)
    {
        // TODO: 실제 점핏 회사 정보 크롤링 구현
        throw new NotImplementedException();
    }
    
    private static void SetupListingHeaders(HttpRequestMessage request, string referer)
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

    private static void SetupDetailHeaders(HttpRequestMessage request, string referer)
    {
        request.Headers.Clear();
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("RSC", "1");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Whale/4.32.315.22 Safari/537.36");
        request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"136\", \"Whale\";v=\"4\", \"Not.A/Brand\";v=\"99\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("Next-Router-State-Tree",
            "%5B%22%22%2C%7B%22children%22%3A%5B%22(main)%22%2C%7B%22children%22%3A%5B%22positions%22%2C%7B%22children%22%3A%5B%22__PAGE__%3F%7B%5C%22sort%5C%22%3A%5C%22popular%5C%22%7D%22%2C%7B%7D%5D%7D%5D%7D%2Cnull%2Cnull%2Ctrue%5D%7D%2Cnull%2Cnull%2Ctrue%5D");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-Mode", "cors");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Referer", referer);
    }
}