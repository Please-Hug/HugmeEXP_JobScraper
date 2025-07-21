using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;

namespace JobScraper.Server.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _companyRepository.GetByIdAsync(id);
    }

    public async Task<Company?> GetByNameAsync(string name)
    {
        return await _companyRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _companyRepository.GetAllAsync();
    }

    public async Task<Company> CreateAsync(Company company)
    {
        if (await _companyRepository.GetByNameAsync(company.Name) != null)
        {
            throw new InvalidOperationException($"Company with name '{company.Name}' already exists");
        }

        return await _companyRepository.AddAsync(company);
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        if (!await _companyRepository.ExistsAsync(company.Id))
        {
            throw new ArgumentException($"Company with ID {company.Id} not found");
        }

        return await _companyRepository.UpdateAsync(company);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _companyRepository.ExistsAsync(id))
        {
            throw new ArgumentException($"Company with ID {id} not found");
        }

        await _companyRepository.DeleteAsync(id);
    }

    public async Task<Company> GetOrCreateByNameAsync(string name)
    {
        var existingCompany = await _companyRepository.GetByNameAsync(name);
        if (existingCompany != null)
        {
            return existingCompany;
        }

        var newCompany = new Company
        {
            Id = 0, // EF will generate the ID
            Name = name
        };

        return await _companyRepository.AddAsync(newCompany);
    }
}
