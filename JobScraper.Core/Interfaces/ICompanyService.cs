using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface ICompanyService
{
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> GetByNameAsync(string name);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> CreateAsync(Company company);
    Task<Company> UpdateAsync(Company company);
    Task DeleteAsync(int id);
    Task<Company> GetOrCreateByNameAsync(string name);
    Task<Company> GetOrCreateCompanyAsync(Company companyInfo);
    Task<Company?> GetBySourceCompanyIdAsync(string sourceCompanyId);
}
