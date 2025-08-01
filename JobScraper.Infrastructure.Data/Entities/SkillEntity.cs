﻿using System.ComponentModel.DataAnnotations;

namespace JobScraper.Infrastructure.Data.Entities;

public class SkillEntity
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; }   // 한글명
    [MaxLength(1000)]
    public string? IconUrl { get; set; }      // 아이콘 URL
    
    // 다대다 관계를 위한 네비게이션 프로퍼티
    public ICollection<JobDetailEntity> JobDetails { get; set; } = new List<JobDetailEntity>();
}
