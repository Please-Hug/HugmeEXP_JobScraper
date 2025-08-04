using JobScraper.Infrastructure.Data;
using JobScraper.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobScraper.Infrastructure.Tests.Repositories
{
    public class SkillRepositoryTests : IDisposable
    {
        private readonly JobScraperDbContext _context;

        public SkillRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<JobScraperDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new JobScraperDbContext(options);
        }

        [Fact]
        public void SkillRepository_ShouldBeCreatable()
        {
            // Arrange & Act
            var repository = new SkillRepository(_context);

            // Assert
            Assert.NotNull(repository);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
