using JobScraper.Core.Interfaces;
using JobScraper.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobScraper.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetAllCompanies()
    {
        try
        {
            var companies = await _companyService.GetAllAsync();
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 목록 조회 중 오류 발생");
            return StatusCode(500, new { Message = "회사 목록 조회 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(new { Message = $"ID {id}인 회사를 찾을 수 없습니다." });
            }
            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 조회 중 오류 발생: {companyId}", id);
            return StatusCode(500, new { Message = "회사 조회 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Company>> GetCompanyByName(string name)
    {
        try
        {
            var company = await _companyService.GetByNameAsync(name);
            if (company == null)
            {
                return NotFound(new { Message = $"'{name}' 회사를 찾을 수 없습니다." });
            }
            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 이름으로 조회 중 오류 발생: {companyName}", name);
            return StatusCode(500, new { Message = "회사 조회 중 오류가 발생했습니다." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Company>> CreateCompany([FromBody] Company company)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCompany = await _companyService.CreateAsync(company);
            return CreatedAtAction(nameof(GetCompany), new { id = createdCompany.Id }, createdCompany);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 생성 중 오류 발생: {companyName}", company.Name);
            return StatusCode(500, new { Message = "회사 생성 중 오류가 발생했습니다." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Company>> UpdateCompany(int id, [FromBody] Company company)
    {
        try
        {
            if (id != company.Id)
            {
                return BadRequest(new { Message = "URL의 ID와 요청 본문의 ID가 일치하지 않습니다." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedCompany = await _companyService.UpdateAsync(company);
            return Ok(updatedCompany);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 업데이트 중 오류 발생: {companyId}", id);
            return StatusCode(500, new { Message = "회사 업데이트 중 오류가 발생했습니다." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCompany(int id)
    {
        try
        {
            await _companyService.DeleteAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 삭제 중 오류 발생: {companyId}", id);
            return StatusCode(500, new { Message = "회사 삭제 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Company>>> SearchCompanies([FromQuery] string? query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var allCompanies = await _companyService.GetAllAsync();
                return Ok(allCompanies);
            }

            // 이름으로 부분 검색 (향후 더 정교한 검색 로직으로 확장 가능)
            var companies = await _companyService.GetAllAsync();
            var filteredCompanies = companies.Where(c => 
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (c.Address != null && c.Address.Contains(query, StringComparison.OrdinalIgnoreCase))
            );

            return Ok(filteredCompanies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회사 검색 중 오류 발생: {query}", query);
            return StatusCode(500, new { Message = "회사 검색 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("health")]
    public ActionResult HealthCheck()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
