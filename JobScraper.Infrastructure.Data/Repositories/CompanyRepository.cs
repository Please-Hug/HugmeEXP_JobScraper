using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly JobScraperDbContext _context;

    public CompanyRepository(JobScraperDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        var entity = await _context.Companies.FindAsync(id);
        return entity?.ToModel();
    }

    public async Task<Company?> GetByNameAsync(string name)
    {
        var entity = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == name);
        return entity?.ToModel();
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        var entities = await _context.Companies.ToListAsync();
        return entities.Select(e => e.ToModel());
    }

    public async Task<Company> AddAsync(Company company)
    {
        var entity = company.ToEntity();
        _context.Companies.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToModel();
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        var entity = await _context.Companies.FindAsync(company.Id);
        if (entity == null)
            throw new ArgumentException($"Company with ID {company.Id} not found");

        entity.Name = company.Name;
        entity.Address = company.Address;
        entity.Latitude = company.Latitude;
        entity.Longitude = company.Longitude;
        entity.EstablishedDate = company.EstablishedDate;
        entity.ImageUrl = company.ImageUrl;
        entity.SourceCompanyId = company.SourceCompanyId;

        await _context.SaveChangesAsync();
        return entity.ToModel();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Companies.FindAsync(id);
        if (entity != null)
        {
            _context.Companies.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Companies.AnyAsync(c => c.Id == id);
    }
}

// Extension methods for mapping between Company and CompanyEntity
public static class CompanyMappingExtensions
{
    public static Company ToModel(this CompanyEntity entity)
    {
        return new Company
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            EstablishedDate = entity.EstablishedDate,
            ImageUrl = entity.ImageUrl,
            SourceCompanyId = entity.SourceCompanyId
        };
    }

    public static CompanyEntity ToEntity(this Company model)
    {
        return new CompanyEntity
        {
            Id = model.Id,
            Name = model.Name,
            Address = model.Address,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            EstablishedDate = model.EstablishedDate,
            ImageUrl = model.ImageUrl,
            SourceCompanyId = model.SourceCompanyId
        };
    }
}
