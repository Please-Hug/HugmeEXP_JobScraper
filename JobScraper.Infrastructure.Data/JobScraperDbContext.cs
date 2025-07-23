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
    public DbSet<TagEntity> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MySQL 행 크기 제한 해결을 위한 TEXT 타입 설정
        modelBuilder.Entity<JobDetailEntity>()
            .Property(jd => jd.Description)
            .HasColumnType("TEXT");
            
        modelBuilder.Entity<JobDetailEntity>()
            .Property(jd => jd.Requirements)
            .HasColumnType("TEXT");
            
        modelBuilder.Entity<JobDetailEntity>()
            .Property(jd => jd.PreferredQualifications)
            .HasColumnType("TEXT");
            
        modelBuilder.Entity<JobDetailEntity>()
            .Property(jd => jd.Benefits)
            .HasColumnType("TEXT");

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
        
        // TagEntity와 JobDetailEntity 간의 다대다 관계 설정
        modelBuilder.Entity<JobDetailEntity>()
            .HasMany(jd => jd.Tags)
            .WithMany(t => t.JobDetails);

        // 인덱스 설정
        modelBuilder.Entity<SkillEntity>()
            .HasIndex(s => s.Name)
            .IsUnique();
        
        modelBuilder.Entity<TagEntity>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
}
