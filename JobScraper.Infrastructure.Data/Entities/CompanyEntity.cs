﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobScraper.Infrastructure.Data.Entities;

/// <summary>
/// 회사 정보 테이블 엔티티
/// </summary>
public class CompanyEntity
{
  [Key]
  public required int Id { get; set; }
  
  [MaxLength(200)]
  public required string Name { get; set; }
  
  [MaxLength(100)]
  public string? SourceCompanyId { get; set; }
  
  [MaxLength(500)]
  public string? Address { get; set; }
  
  [MaxLength(1000)]
  public string? ImageUrl { get; set; }
  
  [Column(TypeName = "decimal(10, 8)")]
  public decimal? Latitude { get; set; }
  
  [Column(TypeName = "decimal(11, 8)")]
  public decimal? Longitude { get; set; }
  public DateTime? EstablishedDate { get; set; }
  
  // Navigation property
  public ICollection<JobListingEntity> JobListings { get; set; } = new List<JobListingEntity>();
}
