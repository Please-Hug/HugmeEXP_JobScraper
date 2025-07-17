using JobScraper.Bot.Scrapers;
using JobScraper.Bot.Services;
using JobScraper.Core;
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
builder.Services.AddSingleton<IHttpClient, DefaultHttpClient>();
builder.Services.AddSingleton<IQueueClient, RabbitMQClient>();
builder.Services.AddKeyedSingleton<IJobScraper, WantedScraper>("wanted");
builder.Services.AddKeyedSingleton<IJobScraper, JumpitScraper>("jumpit");
builder.Services.AddSingleton<IQueueClient>(_ =>
    RabbitMQClient.CreateAsync("localhost", "job-scraper-commands").GetAwaiter().GetResult());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
