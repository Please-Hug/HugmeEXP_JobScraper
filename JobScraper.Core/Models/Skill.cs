namespace JobScraper.Core.Models;

public class Skill
{
    public required int Id { get; set; }
    public required string Name { get; set; }  // 영문명
    public string? IconUrl { get; set; }      // 아이콘 URL
}
