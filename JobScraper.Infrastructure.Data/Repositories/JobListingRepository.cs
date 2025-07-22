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
        var entities = await _context.JobListings
            .Include(jl => jl.Company)
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    public async Task<JobListing?> GetByIdAsync(int id)
    {
        var entity = await _context.JobListings
            .Include(jl => jl.Company)
            .FirstOrDefaultAsync(jl => jl.Id == id);
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<JobListing?> GetByUrlAsync(string url)
    {
        var entity = await _context.JobListings
            .Include(jl => jl.Company)
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
        entity.CompanyId = jobListing.Company.Id;
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
            .Include(jl => jl.Company)
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
            Company = new Company
            {
                Id = entity.Company.Id,
                Name = entity.Company.Name,
                Address = entity.Company.Address,
                Latitude = entity.Company.Latitude,
                Longitude = entity.Company.Longitude,
                EstablishedDate = entity.Company.EstablishedDate,
                ImageUrl = entity.Company.ImageUrl,
                SourceCompanyId = entity.Company.SourceCompanyId
            },
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
            CompanyId = model.Company.Id,
            Company = new CompanyEntity
            {
                Id = model.Company.Id,
                Name = model.Company.Name,
                Address = model.Company.Address,
                Latitude = model.Company.Latitude,
                Longitude = model.Company.Longitude,
                EstablishedDate = model.Company.EstablishedDate,
                ImageUrl = model.Company.ImageUrl,
                SourceCompanyId = model.Company.SourceCompanyId
            },
            Url = model.Url,
            Source = model.Source
        };
    }
}
