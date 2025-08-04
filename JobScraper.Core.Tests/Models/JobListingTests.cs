using JobScraper.Core.Models;
using Xunit;

namespace JobScraper.Core.Tests.Models
{
    public class JobListingTests
    {
        [Fact]
        public void JobListing_WhenCreatedWithRequiredProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var jobListing = new JobListing
            {
                Title = "Software Engineer",
                Url = "https://example.com/job/1",
                Source = "TestSource"
            };

            // Assert
            Assert.Equal("Software Engineer", jobListing.Title);
            Assert.Equal("https://example.com/job/1", jobListing.Url);
            Assert.Equal("TestSource", jobListing.Source);
            Assert.Null(jobListing.Id);
            Assert.Null(jobListing.SourceJobId);
            Assert.Null(jobListing.Company);
        }

        [Fact]
        public void JobListing_WhenCreatedWithAllProperties_ShouldSetAllPropertiesCorrectly()
        {
            // Arrange
            var company = new Company { Name = "Test Company" };

            // Act
            var jobListing = new JobListing
            {
                Id = 1,
                SourceJobId = "EXT123",
                Title = "Senior Developer",
                Company = company,
                Url = "https://example.com/job/senior",
                Source = "JobBoard"
            };

            // Assert
            Assert.Equal(1, jobListing.Id);
            Assert.Equal("EXT123", jobListing.SourceJobId);
            Assert.Equal("Senior Developer", jobListing.Title);
            Assert.Equal(company, jobListing.Company);
            Assert.Equal("https://example.com/job/senior", jobListing.Url);
            Assert.Equal("JobBoard", jobListing.Source);
        }
    }
}
