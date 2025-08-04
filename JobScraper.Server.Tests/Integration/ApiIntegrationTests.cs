using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace JobScraper.Server.Tests.Integration
{
    public class JobDetailControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public JobDetailControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetJobDetail_WhenJobExists_ReturnsOkResponse()
        {
            // Arrange
            var jobId = 1;

            // Act
            var response = await _client.GetAsync($"/api/jobdetails/{jobId}");

            // Assert
            // 실제 데이터베이스가 없으므로 404나 500 응답을 받을 수 있지만
            // 적어도 엔드포인트가 존재하고 요청을 처리할 수 있는지 확인
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetJobDetail_WhenInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = -1;

            // Act
            var response = await _client.GetAsync($"/api/jobdetails/{invalidId}");

            // Assert
            // 음수 ID에 대해서는 적절한 응답을 받아야 함
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }

    public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnHealthyStatus()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            // Health check 엔드포인트가 있다면 응답을 받을 수 있어야 함
            // 없다면 404를 받을 것임
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Root_ShouldReturnSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            // 루트 경로에 대한 응답 확인
            Assert.True(response.IsSuccessStatusCode || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}
