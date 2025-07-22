using JobScraper.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobScraper.Infrastructure.Data;

public class JobScraperDbContext : DbContext
{
    public JobScraperDbContext(DbContextOptions<JobScraperDbContext> options) : base(options)
    {
    }

    public DbSet<JobListingEntity> JobListings { get; set; }
    public DbSet<JobDetailEntity> JobDetails { get; set; }
    public DbSet<SkillEntity> Skills { get; set; }
    public DbSet<CompanyEntity> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CompanyEntity와 JobListingEntity 간의 일대다 관계 설정
        modelBuilder.Entity<JobListingEntity>()
            .HasOne(jl => jl.Company)
            .WithMany(c => c.JobListings)
            .HasForeignKey(jl => jl.CompanyId);

        // JobListingEntity와 JobDetailEntity 간의 일대일 관계 설정
        modelBuilder.Entity<JobListingEntity>()
            .HasOne(jl => jl.JobDetail)
            .WithOne(jd => jd.JobListing)
            .HasForeignKey<JobDetailEntity>(jd => jd.JobListingId);

        // JobDetailEntity와 SkillEntity 간의 다대다 관계 설정
        modelBuilder.Entity<JobDetailEntity>()
            .HasMany(jd => jd.RequiredSkills)
            .WithMany(s => s.JobDetails);

        // 인덱스 설정
        modelBuilder.Entity<SkillEntity>()
            .HasIndex(s => s.EnglishName)
            .IsUnique();
    }
}
