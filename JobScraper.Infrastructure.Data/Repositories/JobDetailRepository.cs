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

    public async Task<IEnumerable<JobDetail>> GetAllAsync()
    {
        var entities = await _context.JobDetails
            .Include(jd => jd.JobListing)
            .Include(jd => jd.RequiredSkills)
            .ToListAsync();
        return entities.Select(MapToModel);
    }

    public async Task<JobDetail?> GetByIdAsync(int id)
    {
        var entity = await _context.JobDetails
            .Include(jd => jd.JobListing)
            .Include(jd => jd.RequiredSkills)
            .FirstOrDefaultAsync(jd => jd.Id == id);
        
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<JobDetail> CreateAsync(JobDetail jobDetail)
    {
        var entity = await MapToEntityAsync(jobDetail);
        _context.JobDetails.Add(entity);
        await _context.SaveChangesAsync();
        
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
        entity.Salary = jobDetail.Salary;
        entity.EducationLevel = jobDetail.EducationLevel;
        entity.Location = jobDetail.Location;
        entity.Prefers = jobDetail.Prefers.ToList();
        entity.Tags = jobDetail.Tags.ToList();
        entity.Qualifications = jobDetail.Qualifications.ToList();

        // 스킬 관계 업데이트 - 문자열을 스킬 엔티티로 변환
        entity.RequiredSkills.Clear();
        
        if (jobDetail.RequiredSkills.Count != 0)
        {
            foreach (var skillName in jobDetail.RequiredSkills)
            {
                var skillEntity = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name == skillName);
                
                if (skillEntity == null)
                {
                    skillEntity = new SkillEntity { Name = skillName };
                    _context.Skills.Add(skillEntity);
                    await _context.SaveChangesAsync();
                }
                
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
        
        if (jobDetail == null)
            throw new ArgumentException($"JobDetail with Id {jobDetailId} not found");

        var skill = await _context.Skills.FindAsync(skillId);
        if (skill == null)
            throw new ArgumentException($"Skill with Id {skillId} not found");

        if (!jobDetail.RequiredSkills.Contains(skill))
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
        
        if (jobDetail == null)
            throw new ArgumentException($"JobDetail with Id {jobDetailId} not found");

        var skill = jobDetail.RequiredSkills.FirstOrDefault(s => s.Id == skillId);
        if (skill != null)
        {
            jobDetail.RequiredSkills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }

    private JobDetail MapToModel(JobDetailEntity entity)
    {
        return new JobDetail
        {
            Id = entity.Id,
            Title = entity.JobListing.Title,
            Company = entity.JobListing.Company,
            Experience = entity.JobListing.Experience,
            Url = entity.JobListing.Url,
            Source = entity.JobListing.Source,
            Description = entity.Description,
            RequiredSkills = entity.RequiredSkills.Select(s => s.Name).ToList(),
            Salary = entity.Salary,
            EducationLevel = entity.EducationLevel,
            Prefers = entity.Prefers.ToList(),
            Tags = entity.Tags.ToList(),
            Qualifications = entity.Qualifications.ToList(),
            Location = entity.Location
        };
    }

    private async Task<JobDetailEntity> MapToEntityAsync(JobDetail model)
    {
        // JobListing을 먼저 찾아옴
        var jobListing = await _context.JobListings.FindAsync(model.Id);
        if (jobListing == null)
            throw new ArgumentException($"JobListing with Id {model.Id} not found", nameof(model));

        var entity = new JobDetailEntity
        {
            JobListingId = model.Id,
            JobListing = jobListing,
            Description = model.Description,
            Salary = model.Salary,
            EducationLevel = model.EducationLevel,
            Location = model.Location,
            Prefers = model.Prefers.ToList(),
            Tags = model.Tags.ToList(),
            Qualifications = model.Qualifications.ToList(),
            RequiredSkills = new List<SkillEntity>()
        };

        // 문자열 스킬들을 스킬 엔티티로 변환
        if (model.RequiredSkills.Count != 0)
        {
            foreach (var skillName in model.RequiredSkills)
            {
                var skillEntity = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name == skillName);
                
                if (skillEntity == null)
                {
                    skillEntity = new SkillEntity { Name = skillName };
                    _context.Skills.Add(skillEntity);
                    await _context.SaveChangesAsync();
                }
                
                entity.RequiredSkills.Add(skillEntity);
            }
        }

        return entity;
    }
}
