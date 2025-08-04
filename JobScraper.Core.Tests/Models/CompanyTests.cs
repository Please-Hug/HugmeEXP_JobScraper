using JobScraper.Core.Models;
using Xunit;

namespace JobScraper.Core.Tests.Models
{
    public class CompanyTests
    {
        [Fact]
        public void Company_WhenCreatedWithRequiredProperties_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var company = new Company
            {
                Name = "테크 컴퍼니"
            };

            // Assert
            Assert.Equal("테크 컴퍼니", company.Name);
            Assert.Null(company.Id);
            Assert.Null(company.Address);
            Assert.Null(company.Latitude);
            Assert.Null(company.Longitude);
        }

        [Fact]
        public void Company_WhenCreatedWithAllProperties_ShouldSetAllPropertiesCorrectly()
        {
            // Arrange
            var establishedDate = new DateTime(2010, 5, 15);

            // Act
            var company = new Company
            {
                Id = 1,
                SourceCompanyId = "COMP123",
                Name = "혁신 기술",
                Address = "경기도 성남시 분당구",
                Latitude = 37.3838m,
                Longitude = 127.1209m,
                EstablishedDate = establishedDate,
                ImageUrl = "https://example.com/logo.png",
                Description = "혁신적인 기술 솔루션을 제공하는 회사입니다."
            };

            // Assert
            Assert.Equal(1, company.Id);
            Assert.Equal("COMP123", company.SourceCompanyId);
            Assert.Equal("혁신 기술", company.Name);
            Assert.Equal("경기도 성남시 분당구", company.Address);
            Assert.Equal(37.3838m, company.Latitude);
            Assert.Equal(127.1209m, company.Longitude);
            Assert.Equal(establishedDate, company.EstablishedDate);
            Assert.Equal("https://example.com/logo.png", company.ImageUrl);
            Assert.Equal("혁신적인 기술 솔루션을 제공하는 회사입니다.", company.Description);
        }
    }
}
