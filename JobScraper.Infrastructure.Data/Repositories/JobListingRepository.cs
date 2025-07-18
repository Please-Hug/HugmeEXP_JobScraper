using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

public class JobListingRepository : IJobListingRepository
{
    private readonly JobScraperDbContext _context;

    public JobListingRepository(JobScraperDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobListing>> GetAllAsync()
    {
        var entities = await _context.JobListings.ToListAsync();
        return entities.Select(MapToModel);
    }

    public async Task<JobListing?> GetByIdAsync(int id)
    {
        var entity = await _context.JobListings.FindAsync(id);
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<JobListing?> GetByUrlAsync(string url)
    {
        var entity = await _context.JobListings
            .FirstOrDefaultAsync(jl => jl.Url == url);
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<JobListing> CreateAsync(JobListing jobListing)
    {
        var entity = MapToEntity(jobListing);
        _context.JobListings.Add(entity);
        await _context.SaveChangesAsync();
        
        jobListing.Id = entity.Id;
        return jobListing;
    }

    public async Task<JobListing> UpdateAsync(JobListing jobListing)
    {
        var entity = await _context.JobListings.FindAsync(jobListing.Id);
        if (entity == null)
            throw new ArgumentException("JobListing not found", nameof(jobListing));

        entity.Title = jobListing.Title;
        entity.Company = jobListing.Company;
        entity.Url = jobListing.Url;
        entity.Source = jobListing.Source;

        await _context.SaveChangesAsync();
        return jobListing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.JobListings.FindAsync(id);
        if (entity != null)
        {
            _context.JobListings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<JobListing>> GetBySourceAsync(string source)
    {
        var entities = await _context.JobListings
            .Where(jl => jl.Source == source)
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    public async Task<bool> ExistsAsync(string url)
    {
        return await _context.JobListings.AnyAsync(jl => jl.Url == url);
    }

    private static JobListing MapToModel(JobListingEntity entity)
    {
        return new JobListing
        {
            Id = entity.Id,
            Title = entity.Title,
            Company = entity.Company,
            Experience = entity.Experience,
            Url = entity.Url,
            Source = entity.Source
        };
    }

    private static JobListingEntity MapToEntity(JobListing model)
    {
        return new JobListingEntity
        {
            Id = model.Id,
            Title = model.Title,
            Company = model.Company,
            Experience = model.Experience,
            Url = model.Url,
            Source = model.Source
        };
    }
}
