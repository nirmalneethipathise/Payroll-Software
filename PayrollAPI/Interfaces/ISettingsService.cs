using PayrollAPI.DTOs.Settings;

namespace PayrollAPI.Interfaces;

public interface ISettingsService
{
    Task<CompanyProfileDto> GetCompanyProfileAsync(int companyId);
    Task<CompanyProfileDto> UpdateCompanyProfileAsync(int companyId, UpdateCompanyProfileDto dto);
    Task<SalaryRuleDto> GetSalaryRulesAsync(int companyId);
    Task<SalaryRuleDto> UpdateSalaryRulesAsync(int companyId, UpdateSalaryRuleDto dto);
}
