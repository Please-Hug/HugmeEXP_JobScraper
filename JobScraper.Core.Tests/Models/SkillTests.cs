using JobScraper.Core.Models;
using Xunit;

namespace JobScraper.Core.Tests.Models
{
    public class SkillTests
    {
        [Fact]
        public void Skill_WhenCreatedWithRequiredProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var skill = new Skill
            {
                Id = 1,
                Name = "C#"
            };

            // Assert
            Assert.Equal(1, skill.Id);
            Assert.Equal("C#", skill.Name);
            Assert.Null(skill.IconUrl);
        }

        [Fact]
        public void Skill_WhenCreatedWithAllProperties_ShouldSetAllPropertiesCorrectly()
        {
            // Arrange & Act
            var skill = new Skill
            {
                Id = 2,
                Name = "JavaScript",
                IconUrl = "https://example.com/js-icon.png"
            };

            // Assert
            Assert.Equal(2, skill.Id);
            Assert.Equal("JavaScript", skill.Name);
            Assert.Equal("https://example.com/js-icon.png", skill.IconUrl);
        }
    }
}
