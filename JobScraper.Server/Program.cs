using JobScraper.Infrastructure.Data;
using JobScraper.Infrastructure.Data.Repositories;
using JobScraper.Core.Interfaces;
using JobScraper.Server.Services;
using JobScraper.Infrastructure.Messaging.Clients;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Entity Framework 설정
builder.Services.AddDbContext<JobScraperDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new MySqlServerVersion(new Version(8, 0, 21))));

// Repository DI 설정
builder.Services.AddScoped<IJobListingRepository, JobListingRepository>();
builder.Services.AddScoped<IJobDetailRepository, JobDetailRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Service DI 설정
builder.Services.AddScoped<IJobListingService, JobListingService>();
builder.Services.AddScoped<IJobDetailService, JobDetailService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ITagService, TagService>();

// RabbitMQ 클라이언트 설정
builder.Services.AddSingleton<IQueueClient>(_ =>
    RabbitMQClient.CreateAsync("localhost", "job-scraper-commands").GetAwaiter().GetResult());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 압축 미들웨어 활성화
app.UseResponseCompression();

app.MapControllers();

app.Run();
