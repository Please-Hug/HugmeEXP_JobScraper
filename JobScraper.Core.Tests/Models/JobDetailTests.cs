using JobScraper.Core.Models;
using Xunit;

namespace JobScraper.Core.Tests.Models
{
    public class JobDetailTests
    {
        [Fact]
        public void JobDetail_WhenCreatedWithAllRequiredProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var skills = new List<Skill> 
            { 
                new Skill { Id = 1, Name = "C#" },
                new Skill { Id = 2, Name = "ASP.NET" }
            };
            var tags = new List<Tag> 
            { 
                new Tag { Name = "Backend" },
                new Tag { Name = "Remote" }
            };

            // Act
            var jobDetail = new JobDetail
            {
                Title = "Senior C# Developer",
                Url = "https://example.com/job/senior-csharp",
                Source = "TechJobs",
                Description = "Looking for an experienced C# developer",
                RequiredSkills = skills,
                MinSalary = 50000000,
                MaxSalary = 80000000,
                Location = "서울시 강남구",
                Tags = tags
            };

            // Assert
            Assert.Equal("Senior C# Developer", jobDetail.Title);
            Assert.Equal("https://example.com/job/senior-csharp", jobDetail.Url);
            Assert.Equal("TechJobs", jobDetail.Source);
            Assert.Equal("Looking for an experienced C# developer", jobDetail.Description);
            Assert.Equal(2, jobDetail.RequiredSkills.Count);
            Assert.Equal(50000000, jobDetail.MinSalary);
            Assert.Equal(80000000, jobDetail.MaxSalary);
            Assert.Equal("서울시 강남구", jobDetail.Location);
            Assert.Equal(2, jobDetail.Tags.Count);
        }

        [Fact]
        public void JobDetail_InheritsFromJobListing_ShouldHaveBaseProperties()
        {
            // Arrange & Act
            var jobDetail = new JobDetail
            {
                Title = "Test Job",
                Url = "https://example.com/test",
                Source = "TestSource",
                Description = "Test Description",
                RequiredSkills = new List<Skill>(),
                MinSalary = 30000000,
                MaxSalary = 50000000,
                Location = "서울"
            };

            // Assert - JobDetail는 JobListing을 상속받으므로 기본 속성들도 가져야 함
            Assert.IsAssignableFrom<JobListing>(jobDetail);
            Assert.Equal("Test Job", jobDetail.Title);
            Assert.Equal("https://example.com/test", jobDetail.Url);
            Assert.Equal("TestSource", jobDetail.Source);
        }

        [Fact]
        public void JobDetail_WhenOptionalPropertiesSet_ShouldRetainValues()
        {
            // Arrange
            var dueDate = DateTime.Now.AddDays(30);
            var company = new Company { Name = "Test Company" };

            // Act
            var jobDetail = new JobDetail
            {
                Title = "Full Stack Developer",
                Url = "https://example.com/fullstack",
                Source = "DevJobs",
                Description = "Full stack development position",
                RequiredSkills = new List<Skill>(),
                MinSalary = 40000000,
                MaxSalary = 60000000,
                Location = "부산시",
                DueDate = dueDate,
                Education = 2,
                ExperienceMin = 3,
                ExperienceMax = 5,
                Requirements = "3년 이상 경력",
                PreferredQualifications = "React, Node.js 경험",
                Benefits = "4대보험, 연차",
                LocationLatitude = 35.1796m,
                LocationLongitude = 129.0756m,
                Company = company
            };

            // Assert
            Assert.Equal(dueDate, jobDetail.DueDate);
            Assert.Equal(2, jobDetail.Education);
            Assert.Equal(3, jobDetail.ExperienceMin);
            Assert.Equal(5, jobDetail.ExperienceMax);
            Assert.Equal("3년 이상 경력", jobDetail.Requirements);
            Assert.Equal("React, Node.js 경험", jobDetail.PreferredQualifications);
            Assert.Equal("4대보험, 연차", jobDetail.Benefits);
            Assert.Equal(35.1796m, jobDetail.LocationLatitude);
            Assert.Equal(129.0756m, jobDetail.LocationLongitude);
            Assert.Equal(company, jobDetail.Company);
        }
    }
}
