using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using JobScraper.Server.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;

namespace JobScraper.Server.Tests.Services
{
    public class JobDetailServiceTests
    {
        private readonly Mock<IJobDetailRepository> _mockJobDetailRepository;
        private readonly Mock<ISkillService> _mockSkillService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly JobDetailService _jobDetailService;

        public JobDetailServiceTests()
        {
            _mockJobDetailRepository = new Mock<IJobDetailRepository>();
            _mockSkillService = new Mock<ISkillService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            
            // Configuration 설정값 모킹 - 테스트용 더미 값 사용
            _mockConfiguration.Setup(x => x["ExternalApi:BaseUrl"])
                .Returns("http://test-api.example.com/api/v1/recruitments/scrape");
            _mockConfiguration.Setup(x => x["ExternalApi:ApiKey"])
                .Returns("test-api-key-12345");
            
            // HTTP 요청을 모킹하여 성공 응답을 반환하도록 설정
            _mockHttpMessageHandler
                .When("http://test-api.example.com/api/v1/recruitments/scrape")
                .Respond(HttpStatusCode.OK, "application/json", "{ \"success\": true }");
            
            // HttpClientFactory가 모킹된 HttpClient를 반환하도록 설정
            var httpClient = _mockHttpMessageHandler.ToHttpClient();
            _mockHttpClientFactory
                .Setup(x => x.CreateClient("ExternalService"))
                .Returns(httpClient);
                
            _jobDetailService = new JobDetailService(
                _mockJobDetailRepository.Object,
                _mockSkillService.Object,
                _mockHttpClientFactory.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetJobDetailByIdAsync_WhenJobExists_ReturnsJobDetail()
        {
            // Arrange
            var jobId = 1;
            var expectedJobDetail = new JobDetail
            {
                Id = jobId,
                Title = "Test Job",
                Url = "https://example.com/job/1",
                Source = "TestSource",
                Description = "Test Description",
                MinSalary = 30000000,
                MaxSalary = 50000000,
                Location = "서울시 강남구",
                Company = new Company { Name = "Test Company", Description = "Test Company Description" },
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            _mockJobDetailRepository
                .Setup(x => x.GetByIdAsync(jobId))
                .ReturnsAsync(expectedJobDetail);

            // Act
            var result = await _jobDetailService.GetJobDetailByIdAsync(jobId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jobId, result.Id);
            Assert.Equal("Test Job", result.Title);
            _mockJobDetailRepository.Verify(x => x.GetByIdAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task GetJobDetailByIdAsync_WhenJobDoesNotExist_ReturnsNull()
        {
            // Arrange
            var jobId = 999;
            _mockJobDetailRepository
                .Setup(x => x.GetByIdAsync(jobId))
                .ReturnsAsync((JobDetail?)null);

            // Act
            var result = await _jobDetailService.GetJobDetailByIdAsync(jobId);

            // Assert
            Assert.Null(result);
            _mockJobDetailRepository.Verify(x => x.GetByIdAsync(jobId), Times.Once);
        }

        [Fact]
        public async Task CreateJobDetailAsync_WhenValidJobDetail_ReturnsCreatedJobDetail()
        {
            // Arrange
            var company = new Company 
            { 
                Name = "Test Company",
                Address = "서울시 강남구",
                Latitude = 37.5665m,
                Longitude = 126.9780m
            };
            
            var jobDetail = new JobDetail
            {
                Title = "New Job",
                Url = "https://example.com/job/new",
                Source = "TestSource",
                Description = "New Job Description",
                MinSalary = 35000000,
                MaxSalary = 55000000,
                Location = "서울시 서초구",
                Company = company,
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            var createdJobDetail = new JobDetail
            {
                Id = 1,
                Title = "New Job",
                Url = "https://example.com/job/new",
                Source = "TestSource",
                Description = "New Job Description",
                MinSalary = 35000000,
                MaxSalary = 55000000,
                Location = "서울시 서초구",
                Company = company,
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            _mockJobDetailRepository
                .Setup(x => x.CreateAsync(It.IsAny<JobDetail>()))
                .ReturnsAsync(createdJobDetail);

            // PushJobDetailToExternalServiceAsync에서 사용되는 GetByIdAsync도 모킹
            _mockJobDetailRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(createdJobDetail);

            // Act
            var result = await _jobDetailService.CreateJobDetailAsync(jobDetail);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("New Job", result.Title);
            _mockJobDetailRepository.Verify(x => x.CreateAsync(It.IsAny<JobDetail>()), Times.Once);
        }

        [Fact]
        public async Task UpdateJobDetailAsync_WhenJobDetailHasNoId_ThrowsArgumentException()
        {
            // Arrange
            var jobDetail = new JobDetail
            {
                Id = null,
                Title = "Updated Job",
                Url = "https://example.com/job/update",
                Source = "TestSource",
                Description = "Updated Description",
                MinSalary = 40000000,
                MaxSalary = 60000000,
                Location = "서울시 종로구",
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _jobDetailService.UpdateJobDetailAsync(jobDetail));
            
            Assert.Contains("JobDetail ID is required for update operation", exception.Message);
        }

        [Fact]
        public async Task UpdateJobDetailAsync_WhenJobDetailNotFound_ThrowsArgumentException()
        {
            // Arrange
            var jobDetail = new JobDetail
            {
                Id = 1,
                Title = "Updated Job",
                Url = "https://example.com/job/update",
                Source = "TestSource",
                Description = "Updated Description",
                MinSalary = 40000000,
                MaxSalary = 60000000,
                Location = "서울시 종로구",
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            _mockJobDetailRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((JobDetail?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _jobDetailService.UpdateJobDetailAsync(jobDetail));
            
            Assert.Contains("JobDetail with ID 1 not found", exception.Message);
        }

        [Fact]
        public async Task UpdateJobDetailAsync_WhenValidJobDetail_ReturnsUpdatedJobDetail()
        {
            // Arrange
            var company = new Company 
            { 
                Name = "Test Company",
                Address = "서울시 강남구",
                Latitude = 37.5665m,
                Longitude = 126.9780m
            };
            
            var existingJobDetail = new JobDetail
            {
                Id = 1,
                Title = "Old Job",
                Url = "https://example.com/job/old",
                Source = "TestSource",
                Description = "Old Description",
                MinSalary = 30000000,
                MaxSalary = 50000000,
                Location = "서울시 강남구",
                Company = company,
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            var updatedJobDetail = new JobDetail
            {
                Id = 1,
                Title = "Updated Job",
                Url = "https://example.com/job/updated",
                Source = "TestSource",
                Description = "Updated Description",
                MinSalary = 40000000,
                MaxSalary = 60000000,
                Location = "서울시 서초구",
                Company = company,
                RequiredSkills = new List<Skill>(),
                Tags = new List<Tag>()
            };

            // 첫 번째 호출 (UpdateJobDetailAsync에서)
            _mockJobDetailRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingJobDetail);

            _mockJobDetailRepository
                .Setup(x => x.UpdateAsync(It.IsAny<JobDetail>()))
                .ReturnsAsync(updatedJobDetail);

            // Act
            var result = await _jobDetailService.UpdateJobDetailAsync(updatedJobDetail);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated Job", result.Title);
            _mockJobDetailRepository.Verify(x => x.GetByIdAsync(1), Times.AtLeastOnce);
            _mockJobDetailRepository.Verify(x => x.UpdateAsync(It.IsAny<JobDetail>()), Times.Once);
        }
    }
}
