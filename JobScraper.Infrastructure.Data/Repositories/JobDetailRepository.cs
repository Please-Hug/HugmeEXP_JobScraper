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
        if (string.IsNullOrEmpty(jobDetail.SourceJobId))
        {
            throw new ArgumentException("JobDetail SourceJobId is required for creation.");
        }
        
        // 새로운 엔티티 생성
        var entity = await MapToEntityAsync(jobDetail);
        // JobListingId는 이미 MapToEntityAsync에서 설정됨
        _context.JobDetails.Add(entity);
        await _context.SaveChangesAsync();
        
        jobDetail.Id = entity.Id;
        return jobDetail;
    }

    public async Task<JobDetail> UpdateAsync(JobDetail jobDetail)
    {
        var entity = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .Include(jd => jd.Tags)
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
        entity.Tags.Clear();
        
        if (jobDetail.RequiredSkills.Count > 0)
        {
            var skilNames = jobDetail.RequiredSkills.Select(s => s.Name).ToList();
            var skillEntities = await _context.Skills
                .Where(s => skilNames.Contains(s.Name))
                .ToListAsync();

            foreach (var skillEntity in skillEntities)
            {
                entity.RequiredSkills.Add(skillEntity);
            }
            
            var existingSkillNames = new HashSet<string>(skillEntities.Select(e => e.Name));
            var newSkillEntities = jobDetail.RequiredSkills
                .Where(s => !existingSkillNames.Contains(s.Name))
                .Select(s => new SkillEntity { Name = s.Name, IconUrl = s.IconUrl })
                .ToList();

            foreach (var newSkillEntity in newSkillEntities)
            {
                entity.RequiredSkills.Add(newSkillEntity);
            }
        }

        if (jobDetail.Tags.Count > 0)
        {
            var tagNames = jobDetail.Tags.Select(t => t.Name).ToList();
            var tagEntities = await _context.Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToListAsync();

            foreach (var tagEntity in tagEntities)
            {
                entity.Tags.Add(tagEntity);
            }
            
            var existingTagNames = new HashSet<string>(tagEntities.Select(e => e.Name));
            var newTagEntities = jobDetail.Tags
                .Where(t => !existingTagNames.Contains(t.Name))
                .Select(t => new TagEntity { Name = t.Name })
                .ToList();

            foreach (var newTagEntity in newTagEntities)
            {
                entity.Tags.Add(newTagEntity);
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

    public async Task<JobDetail?> GetByJobListingIdAsync(int id)
    {
        var entity = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .Include(jd => jd.Tags)
            .Include(jd => jd.JobListing)
                .ThenInclude(jl => jl.Company)
            .FirstOrDefaultAsync(jd => jd.JobListingId == id);
        
        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<IEnumerable<JobDetail>> GetAllAsync()
    {
        var entities = await _context.JobDetails
            .Include(jd => jd.RequiredSkills)
            .Include(jd => jd.Tags)
            .Include(jd => jd.JobListing)
            .ThenInclude(jl => jl.Company)
            .ToListAsync();
        
        return entities.Select(MapToModel);
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
                Name = s.Name,
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
            DueDate = entity.DueDate,
            Tags = entity.Tags.Select(t => new Tag
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }

    private async Task<JobDetailEntity> MapToEntityAsync(JobDetail model)
    {
        if (string.IsNullOrEmpty(model.SourceJobId))
        {
            throw new ArgumentException("JobDetail SourceJobId is required", nameof(model));
        }
        
        // SourceJobId로 JobListing을 찾아옴
        var jobListing = await _context.JobListings.FirstOrDefaultAsync(jl => jl.SourceJobId == model.SourceJobId);
        if (jobListing == null)
            throw new ArgumentException($"JobListing with SourceJobId {model.SourceJobId} not found", nameof(model));

        var entity = new JobDetailEntity
        {
            JobListingId = jobListing.Id, // JobListing의 실제 ID 사용
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
            DueDate = model.DueDate,
            Tags = new List<TagEntity>()
        };

        // 스킬 엔티티들을 찾아서 연결
        if (model.RequiredSkills.Count > 0)
        {
            var skillNames = model.RequiredSkills.Select(s => s.Name.ToLower()).ToList();
            
            var skillEntities = await _context.Skills
                .Where(s => skillNames.Contains(s.Name.ToLower()))
                .ToListAsync();
            var existingSkillNames = new HashSet<string>(skillEntities.Select(e => e.Name.ToLower()));
            var newSkillEntities = model.RequiredSkills
                .Where(s => !existingSkillNames.Contains(s.Name.ToLower()))
                .Select(s => new SkillEntity { Name = s.Name, IconUrl = s.IconUrl })
                .ToList();

            foreach (var skillEntity in skillEntities)
            {
                entity.RequiredSkills.Add(skillEntity);
            }

            foreach (var newSkillEntity in newSkillEntities)
            {
                entity.RequiredSkills.Add(newSkillEntity);
            }
        }

        if (model.Tags.Count > 0)
        {
            var tagNames = model.Tags.Select(t => t.Name.ToLower()).ToList();
            
            var tagEntities = await _context.Tags
                .Where(t => tagNames.Contains(t.Name.ToLower()))
                .ToListAsync();
            var existingTagNames = new HashSet<string>(tagEntities.Select(e => e.Name.ToLower()));
            var newTagEntities = model.Tags
                .Where(t => !existingTagNames.Contains(t.Name.ToLower()))
                .Select(t => new TagEntity { Name = t.Name })
                .ToList();
            
            foreach (var tagEntity in tagEntities) 
            {
                entity.Tags.Add(tagEntity);
            }


            foreach (var newTagEntity in newTagEntities)
            {
                entity.Tags.Add(newTagEntity);
            }
        }
        

        return entity;
    }
}
