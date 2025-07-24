using JobScraper.Core.Models;

namespace JobScraper.Core.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> GetByNameAsync(string name);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> AddAsync(Company company);
    Task<Company> UpdateAsync(Company company);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<Company?> SearchByNameAsync(string trim);
    Task<Company?> GetBySourceCompanyIdAsync(string sourceCompanyId);
    Task<IEnumerable<Company>> GetAllCompaniesNotHavingDetailsAsync();
}
