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
        // SourceCompanyId 우선 확인 후 이름으로 보조 확인
        CompanyEntity? existingCompany = null;
        
        if (!string.IsNullOrEmpty(jobListing.Company.SourceCompanyId))
        {
            existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.SourceCompanyId == jobListing.Company.SourceCompanyId);
        }
        
        if (existingCompany == null)
        {
            existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == jobListing.Company.Name);
        }
        
        if (existingCompany == null)
        {
            // 새 회사 생성
            var companyEntity = new CompanyEntity
            {
                Id = 0,
                Name = jobListing.Company.Name,
                SourceCompanyId = jobListing.Company.SourceCompanyId,
                Address = jobListing.Company.Address,
                ImageUrl = jobListing.Company.ImageUrl,
                Latitude = jobListing.Company.Latitude,
                Longitude = jobListing.Company.Longitude,
                EstablishedDate = jobListing.Company.EstablishedDate
            };
            _context.Companies.Add(companyEntity);
            await _context.SaveChangesAsync();
            existingCompany = companyEntity;
        }
        else
        {
            // 기존 회사 정보 업데이트 (더 완전한 정보가 있다면)
            var needsUpdate = false;
            
            if (!string.IsNullOrEmpty(jobListing.Company.SourceCompanyId) && 
                existingCompany.SourceCompanyId != jobListing.Company.SourceCompanyId)
            {
                existingCompany.SourceCompanyId = jobListing.Company.SourceCompanyId;
                needsUpdate = true;
            }
            
            if (!string.IsNullOrEmpty(jobListing.Company.ImageUrl) && 
                existingCompany.ImageUrl != jobListing.Company.ImageUrl)
            {
                existingCompany.ImageUrl = jobListing.Company.ImageUrl;
                needsUpdate = true;
            }
            
            if (!string.IsNullOrEmpty(jobListing.Company.Address) && 
                existingCompany.Address != jobListing.Company.Address)
            {
                existingCompany.Address = jobListing.Company.Address;
                needsUpdate = true;
            }
            
            if (jobListing.Company.Latitude.HasValue && 
                existingCompany.Latitude != jobListing.Company.Latitude)
            {
                existingCompany.Latitude = jobListing.Company.Latitude;
                needsUpdate = true;
            }
            
            if (jobListing.Company.Longitude.HasValue && 
                existingCompany.Longitude != jobListing.Company.Longitude)
            {
                existingCompany.Longitude = jobListing.Company.Longitude;
                needsUpdate = true;
            }
            
            if (jobListing.Company.EstablishedDate.HasValue && 
                existingCompany.EstablishedDate != jobListing.Company.EstablishedDate)
            {
                existingCompany.EstablishedDate = jobListing.Company.EstablishedDate;
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                await _context.SaveChangesAsync();
            }
        }
        
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
