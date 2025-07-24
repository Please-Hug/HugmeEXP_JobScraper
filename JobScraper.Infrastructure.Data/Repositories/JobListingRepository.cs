using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

/// <summary>
/// 채용공고 데이터 접근 계층
/// </summary>
public class JobListingRepository : IJobListingRepository
{
    private readonly JobScraperDbContext _context;

    public JobListingRepository(JobScraperDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 모든 채용공고 조회
    /// </summary>
    public async Task<IEnumerable<JobListing>> GetAllAsync()
    {
        var entities = await _context.JobListings
            .Include(jl => jl.Company)
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    /// <summary>
    /// ID로 채용공고 조회
    /// </summary>
    public async Task<JobListing?> GetByIdAsync(int id)
    {
        var entity = await _context.JobListings
            .Include(jl => jl.Company)
            .FirstOrDefaultAsync(jl => jl.Id == id);
        return entity != null ? MapToModel(entity) : null;
    }

    /// <summary>
    /// URL로 채용공고 조회
    /// </summary>
    public async Task<JobListing?> GetByUrlAsync(string url)
    {
        var entity = await _context.JobListings
            .Include(jl => jl.Company)
            .FirstOrDefaultAsync(jl => jl.Url == url);
        return entity != null ? MapToModel(entity) : null;
    }

    /// <summary>
    /// 외부 시스템의 Job ID로 채용공고 조회
    /// </summary>
    public async Task<JobListing?> GetBySourceJobIdAsync(string sourceJobId)
    {
        var entity = await _context.JobListings
            .Include(jl => jl.Company)
            .FirstOrDefaultAsync(jl => jl.SourceJobId == sourceJobId);
        return entity != null ? MapToModel(entity) : null;
    }

    /// <summary>
    /// 새 채용공고 생성 (회사 정보 자동 관리)
    /// </summary>
    public async Task<JobListing> CreateAsync(JobListing jobListing)
    {
        var existingCompany = await GetOrCreateCompanyAsync(jobListing.Company);
        
        var entity = new JobListingEntity
        {
            Id = jobListing.Id ?? 0,
            SourceJobId = jobListing.SourceJobId,
            Title = jobListing.Title,
            CompanyId = existingCompany.Id,
            Company = existingCompany,
            Url = jobListing.Url,
            Source = jobListing.Source
        };
        
        _context.JobListings.Add(entity);
        await _context.SaveChangesAsync();
        
        jobListing.Id = entity.Id;
        jobListing.Company.Id = existingCompany.Id;
        return jobListing;
    }

    /// <summary>
    /// 회사를 찾거나 새로 생성합니다
    /// </summary>
    private async Task<CompanyEntity> GetOrCreateCompanyAsync(Company company)
    {
        var existingCompany = await FindExistingCompanyAsync(company);
        
        if (existingCompany == null)
        {
            return await CreateNewCompanyAsync(company);
        }
        
        await UpdateCompanyIfNeededAsync(existingCompany, company);
        return existingCompany;
    }

    /// <summary>
    /// 기존 회사를 찾습니다 (SourceCompanyId 우선, 이름으로 보조)
    /// </summary>
    private async Task<CompanyEntity?> FindExistingCompanyAsync(Company company)
    {
        if (!string.IsNullOrEmpty(company.SourceCompanyId))
        {
            var companyBySourceId = await _context.Companies
                .FirstOrDefaultAsync(c => c.SourceCompanyId == company.SourceCompanyId);
            if (companyBySourceId != null)
                return companyBySourceId;
        }
        
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == company.Name);
    }

    /// <summary>
    /// 새 회사를 생성합니다
    /// </summary>
    private async Task<CompanyEntity> CreateNewCompanyAsync(Company company)
    {
        var companyEntity = new CompanyEntity
        {
            Id = 0,
            Name = company.Name,
            SourceCompanyId = company.SourceCompanyId,
            Address = company.Address,
            ImageUrl = company.ImageUrl,
            Latitude = company.Latitude,
            Longitude = company.Longitude,
            EstablishedDate = company.EstablishedDate
        };
        
        _context.Companies.Add(companyEntity);
        await _context.SaveChangesAsync();
        return companyEntity;
    }

    /// <summary>
    /// 기존 회사 정보를 필요시 업데이트합니다
    /// </summary>
    private async Task UpdateCompanyIfNeededAsync(CompanyEntity existingCompany, Company newCompanyData)
    {
        var needsUpdate = false;
        
        if (!string.IsNullOrEmpty(newCompanyData.SourceCompanyId) && 
            existingCompany.SourceCompanyId != newCompanyData.SourceCompanyId)
        {
            existingCompany.SourceCompanyId = newCompanyData.SourceCompanyId;
            needsUpdate = true;
        }
        
        if (!string.IsNullOrEmpty(newCompanyData.ImageUrl) && 
            existingCompany.ImageUrl != newCompanyData.ImageUrl)
        {
            existingCompany.ImageUrl = newCompanyData.ImageUrl;
            needsUpdate = true;
        }
        
        if (!string.IsNullOrEmpty(newCompanyData.Address) && 
            existingCompany.Address != newCompanyData.Address)
        {
            existingCompany.Address = newCompanyData.Address;
            needsUpdate = true;
        }
        
        if (newCompanyData.Latitude.HasValue && 
            existingCompany.Latitude != newCompanyData.Latitude)
        {
            existingCompany.Latitude = newCompanyData.Latitude;
            needsUpdate = true;
        }
        
        if (newCompanyData.Longitude.HasValue && 
            existingCompany.Longitude != newCompanyData.Longitude)
        {
            existingCompany.Longitude = newCompanyData.Longitude;
            needsUpdate = true;
        }
        
        if (newCompanyData.EstablishedDate.HasValue && 
            existingCompany.EstablishedDate != newCompanyData.EstablishedDate)
        {
            existingCompany.EstablishedDate = newCompanyData.EstablishedDate;
            needsUpdate = true;
        }
        
        if (needsUpdate)
        {
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 채용공고 수정
    /// </summary>
    public async Task<JobListing> UpdateAsync(JobListing jobListing)
    {
        var entity = await _context.JobListings.FindAsync(jobListing.Id);
        if (entity == null)
            throw new ArgumentException("JobListing not found", nameof(jobListing));

        entity.SourceJobId = jobListing.SourceJobId;
        entity.Title = jobListing.Title;
        entity.CompanyId = jobListing.Company.Id ?? throw new ArgumentException("Company ID is required for update");
        entity.Url = jobListing.Url;
        entity.Source = jobListing.Source;

        await _context.SaveChangesAsync();
        return jobListing;
    }

    /// <summary>
    /// 채용공고 삭제
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var entity = await _context.JobListings.FindAsync(id);
        if (entity != null)
        {
            _context.JobListings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 특정 소스의 모든 채용공고 조회
    /// </summary>
    public async Task<IEnumerable<JobListing>> GetBySourceAsync(string source)
    {
        var entities = await _context.JobListings
            .Include(jl => jl.Company)
            .Where(jl => jl.Source == source)
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    /// <summary>
    /// 주어진 URL의 채용공고 존재 여부 확인
    /// </summary>
    public async Task<bool> ExistsAsync(string url)
    {
        return await _context.JobListings.AnyAsync(jl => jl.Url == url);
    }

    public async Task<IEnumerable<JobListing>> GetAllNotHavingDetailsAsync()
    {
        var entities = await _context.JobListings
            .Include(jl => jl.Company)
            .Where(jl => jl.JobDetail == null)
            .ToListAsync();
        
        return entities.Select(MapToModel);
    }

    private static JobListing MapToModel(JobListingEntity entity)
    {
        return new JobListing
        {
            Id = entity.Id,
            SourceJobId = entity.SourceJobId,
            Title = entity.Title,
            Company = new Company
            {
                Id = entity.Company.Id,
                Name = entity.Company.Name,
                SourceCompanyId = entity.Company.SourceCompanyId,
                Address = entity.Company.Address,
                Latitude = entity.Company.Latitude,
                Longitude = entity.Company.Longitude,
                EstablishedDate = entity.Company.EstablishedDate,
                ImageUrl = entity.Company.ImageUrl
            },
            Url = entity.Url,
            Source = entity.Source
        };
    }
}
