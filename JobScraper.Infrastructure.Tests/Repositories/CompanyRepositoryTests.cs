using JobScraper.Infrastructure.Data;
using JobScraper.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobScraper.Infrastructure.Tests.Repositories
{
    public class CompanyRepositoryTests : IDisposable
    {
        private readonly JobScraperDbContext _context;

        public CompanyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<JobScraperDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new JobScraperDbContext(options);
        }

        [Fact]
        public void CompanyRepository_ShouldBeCreatable()
        {
            // Arrange & Act
            var repository = new CompanyRepository(_context);

            // Assert
            Assert.NotNull(repository);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
