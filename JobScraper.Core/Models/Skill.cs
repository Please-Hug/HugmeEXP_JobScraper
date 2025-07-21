namespace JobScraper.Core.Models;

public class Skill
{
    public required int Id { get; set; }
    public required string EnglishName { get; set; }  // 영문명
    public required string KoreanName { get; set; }   // 한글명
    public string? IconUrl { get; set; }      // 아이콘 URL
}
