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
        if (!company.Id.HasValue)
        {
            throw new ArgumentException("Company ID is required for update operation.");
        }
        
        if (!await _companyRepository.ExistsAsync(company.Id.Value))
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

    // SourceCompanyId로 회사를 조회하는 메서드 추가
    public async Task<Company?> GetBySourceCompanyIdAsync(string sourceCompanyId)
    {
        var company = await _companyRepository.GetBySourceCompanyIdAsync(sourceCompanyId);
        return company;
    }

    public Task<Company?> SearchByNameAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<Company?>(null);
        }
        
        return _companyRepository.SearchByNameAsync(query.Trim());
    }

    // 회사 정보를 포괄적으로 처리하는 새 메서드 추가
    public async Task<Company> GetOrCreateCompanyAsync(Company companyInfo)
    {
        Company? existingCompany = null;

        // 먼저 SourceCompanyId로 조회 시도
        if (!string.IsNullOrEmpty(companyInfo.SourceCompanyId))
        {
            existingCompany = await GetBySourceCompanyIdAsync(companyInfo.SourceCompanyId);
        }

        // SourceCompanyId로 찾지 못했다면 이름으로 조회
        if (existingCompany == null)
        {
            existingCompany = await _companyRepository.GetByNameAsync(companyInfo.Name);
        }

        if (existingCompany != null)
        {
            // 기존 회사 정보 업데이트 (새로운 정보가 있는 경우)
            bool needsUpdate = false;

            if (!string.IsNullOrEmpty(companyInfo.SourceCompanyId) && existingCompany.SourceCompanyId != companyInfo.SourceCompanyId)
            {
                existingCompany.SourceCompanyId = companyInfo.SourceCompanyId;
                needsUpdate = true;
            }

            if (!string.IsNullOrEmpty(companyInfo.ImageUrl) && existingCompany.ImageUrl != companyInfo.ImageUrl)
            {
                existingCompany.ImageUrl = companyInfo.ImageUrl;
                needsUpdate = true;
            }

            if (!string.IsNullOrEmpty(companyInfo.Address) && existingCompany.Address != companyInfo.Address)
            {
                existingCompany.Address = companyInfo.Address;
                needsUpdate = true;
            }

            if (companyInfo.Latitude.HasValue && existingCompany.Latitude != companyInfo.Latitude)
            {
                existingCompany.Latitude = companyInfo.Latitude;
                needsUpdate = true;
            }

            if (companyInfo.Longitude.HasValue && existingCompany.Longitude != companyInfo.Longitude)
            {
                existingCompany.Longitude = companyInfo.Longitude;
                needsUpdate = true;
            }

            if (companyInfo.EstablishedDate.HasValue && existingCompany.EstablishedDate != companyInfo.EstablishedDate)
            {
                existingCompany.EstablishedDate = companyInfo.EstablishedDate;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                return await _companyRepository.UpdateAsync(existingCompany);
            }

            return existingCompany;
        }

        // 새 회사 생성
        var newCompany = new Company
        {
            Id = 0, // EF will generate the ID
            Name = companyInfo.Name,
            SourceCompanyId = companyInfo.SourceCompanyId,
            Address = companyInfo.Address,
            ImageUrl = companyInfo.ImageUrl,
            Latitude = companyInfo.Latitude,
            Longitude = companyInfo.Longitude,
            EstablishedDate = companyInfo.EstablishedDate
        };

        return await _companyRepository.AddAsync(newCompany);
    }
}
