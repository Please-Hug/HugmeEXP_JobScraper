using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data.Repositories;

public class JobDetailRepository : IJobDetailRepository
{
    private readonly JobScraperDbContext _context;

    public JobDetailRepository(JobScraperDbContext context)
    {
        _context = context;
    }

    public async Task<JobDetail?> GetByIdAsync(int id)
    {
        var entity = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .Include(jd => jd.JobListing)
                .ThenInclude(jl => jl.Company)
            .FirstOrDefaultAsync(jd => jd.Id == id);
        
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<JobDetail> CreateAsync(JobDetail jobDetail)
    {
        if (!jobDetail.Id.HasValue)
        {
            throw new ArgumentException("JobDetail ID is required for creation.");
        }
        
        var entity = await MapToEntityAsync(jobDetail);
        entity.JobListingId = jobDetail.Id.Value; // 이미 HasValue로 체크했으므로 안전
        _context.JobDetails.Add(entity);
        await _context.SaveChangesAsync();
        
        // ID를 업데이트하고 반환
        jobDetail.Id = entity.Id;
        return jobDetail;
    }

    public async Task<JobDetail> UpdateAsync(JobDetail jobDetail)
    {
        var entity = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .FirstOrDefaultAsync(jd => jd.Id == jobDetail.Id);
        
        if (entity == null)
            throw new ArgumentException("JobDetail not found", nameof(jobDetail));

        // 기본 속성 업데이트
        entity.Description = jobDetail.Description;
        entity.MinSalary = jobDetail.MinSalary;
        entity.MaxSalary = jobDetail.MaxSalary;
        entity.Location = jobDetail.Location;
        
        // 새로 추가된 필드들 업데이트
        entity.Education = jobDetail.Education;
        entity.Experience = jobDetail.Experience;
        entity.Requirements = jobDetail.Requirements;
        entity.PreferredQualifications = jobDetail.PreferredQualifications;
        entity.Benefits = jobDetail.Benefits;
        entity.LocationLatitude = jobDetail.LocationLatitude;
        entity.LocationLongitude = jobDetail.LocationLongitude;
        entity.DueDate = jobDetail.DueDate;

        // 스킬 관계 업데이트 - N+1 문제 해결
        entity.RequiredSkills.Clear();
        
        if (jobDetail.RequiredSkills.Count != 0)
        {
            var skillIds = jobDetail.RequiredSkills.Select(s => s.Id).ToList();
            var skillEntities = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToListAsync();

            foreach (var skillEntity in skillEntities)
            {
                entity.RequiredSkills.Add(skillEntity);
            }
        }

        await _context.SaveChangesAsync();
        return jobDetail;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.JobDetails.FindAsync(id);
        if (entity != null)
        {
            _context.JobDetails.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddSkillToJobAsync(int jobDetailId, int skillId)
    {
        var jobDetail = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .FirstOrDefaultAsync(jd => jd.Id == jobDetailId);
        
        var skill = await _context.Skills.FindAsync(skillId);
        
        if (jobDetail != null && skill != null && !jobDetail.RequiredSkills.Contains(skill))
        {
            jobDetail.RequiredSkills.Add(skill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveSkillFromJobAsync(int jobDetailId, int skillId)
    {
        var jobDetail = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .FirstOrDefaultAsync(jd => jd.Id == jobDetailId);
        
        if (jobDetail != null)
        {
            var skill = jobDetail.RequiredSkills.FirstOrDefault(s => s.Id == skillId);
            if (skill != null)
            {
                jobDetail.RequiredSkills.Remove(skill);
                await _context.SaveChangesAsync();
            }
        }
    }

    private JobDetail MapToModel(JobDetailEntity entity)
    {
        return new JobDetail
        {
            Id = entity.Id,
            SourceJobId = entity.JobListing.SourceJobId,  // 누락된 필드 추가
            Title = entity.JobListing.Title,
            Company = new Company
            {
                Id = entity.JobListing.Company.Id,
                Name = entity.JobListing.Company.Name,
                SourceCompanyId = entity.JobListing.Company.SourceCompanyId,  // 누락된 필드 추가
                Address = entity.JobListing.Company.Address,
                ImageUrl = entity.JobListing.Company.ImageUrl,  // 누락된 필드 추가
                Latitude = entity.JobListing.Company.Latitude,
                Longitude = entity.JobListing.Company.Longitude,
                EstablishedDate = entity.JobListing.Company.EstablishedDate
            },
            Url = entity.JobListing.Url,
            Source = entity.JobListing.Source,
            Description = entity.Description,
            RequiredSkills = entity.RequiredSkills.Select(s => new Skill
            {
                Id = s.Id,
                EnglishName = s.EnglishName,
                KoreanName = s.KoreanName,
                IconUrl = s.IconUrl,
            }).ToList(),
            MinSalary = entity.MinSalary,
            MaxSalary = entity.MaxSalary,
            Location = entity.Location,
            // 새로 추가된 필드들
            Education = entity.Education,
            Experience = entity.Experience,
            Requirements = entity.Requirements,
            PreferredQualifications = entity.PreferredQualifications,
            Benefits = entity.Benefits,
            LocationLatitude = entity.LocationLatitude,
            LocationLongitude = entity.LocationLongitude,
            DueDate = entity.DueDate
        };
    }

    private async Task<JobDetailEntity> MapToEntityAsync(JobDetail model)
    {
        if (!model.Id.HasValue)
        {
            throw new ArgumentException("JobDetail ID is required", nameof(model));
        }
        
        // JobListing을 먼저 찾아옴
        var jobListing = await _context.JobListings.FindAsync(model.Id.Value);
        if (jobListing == null)
            throw new ArgumentException($"JobListing with Id {model.Id} not found", nameof(model));

        var entity = new JobDetailEntity
        {
            JobListingId = model.Id.Value, // 이미 HasValue로 체크했으므로 안전
            JobListing = jobListing,
            Description = model.Description,
            MinSalary = model.MinSalary,
            MaxSalary = model.MaxSalary,
            Location = model.Location,
            // 새로 추가된 필드들
            Education = model.Education,
            Experience = model.Experience,
            Requirements = model.Requirements,
            PreferredQualifications = model.PreferredQualifications,
            Benefits = model.Benefits,
            LocationLatitude = model.LocationLatitude,
            LocationLongitude = model.LocationLongitude,
            RequiredSkills = new List<SkillEntity>(),
            DueDate = model.DueDate
        };

        // 스킬 엔티티들을 찾아서 연결 - N+1 문제 해결
        if (model.RequiredSkills.Count != 0)
        {
            var skillIds = model.RequiredSkills.Select(s => s.Id).ToList();
            var skillEntities = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToListAsync();

            foreach (var skillEntity in skillEntities)
            {
                entity.RequiredSkills.Add(skillEntity);
            }
        }

        return entity;
    }
}
