namespace JobScraper.Core.Models;

public class Company
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; }  // 회사 이미지 URL
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? EstablishedDate { get; set; }
}
