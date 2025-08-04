using JobScraper.Core.Models;
using Xunit;

namespace JobScraper.Core.Tests.Models
{
    public class TagTests
    {
        [Fact]
        public void Tag_WhenCreatedWithRequiredProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var tag = new Tag
            {
                Name = "Remote"
            };

            // Assert
            Assert.Equal("Remote", tag.Name);
            Assert.Null(tag.Id);
        }

        [Fact]
        public void Tag_WhenCreatedWithAllProperties_ShouldSetAllPropertiesCorrectly()
        {
            // Arrange & Act
            var tag = new Tag
            {
                Id = 1,
                Name = "Full-time"
            };

            // Assert
            Assert.Equal(1, tag.Id);
            Assert.Equal("Full-time", tag.Name);
        }
    }
}
