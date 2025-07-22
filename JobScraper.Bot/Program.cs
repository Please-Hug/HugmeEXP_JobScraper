using JobScraper.Bot.Scrapers;
using JobScraper.Bot.Services;
using JobScraper.Core.Interfaces;
using JobScraper.Infrastructure.Http.Clients;
using JobScraper.Infrastructure.Messaging.Clients;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// HttpClientFactory 추가 (WantedScraper에서 사용)
builder.Services.AddHttpClient("Wanted");
builder.Services.AddHttpClient("Jumpit");

// 기존의 커스텀 HttpClient (다른 용도)
builder.Services.AddSingleton<IHttpClient, DefaultHttpClient>();
builder.Services.AddKeyedSingleton<IJobScraper, WantedScraper>("wanted");
builder.Services.AddKeyedSingleton<IJobScraper, JumpitScraper>("jumpit");
builder.Services.AddSingleton<IQueueClient>(_ =>
    RabbitMQClient.CreateAsync("localhost", "job-scraper-commands").GetAwaiter().GetResult());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
