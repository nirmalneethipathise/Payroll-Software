using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Settings;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _db;

    public SettingsService(AppDbContext db) => _db = db;

    public async Task<CompanyProfileDto> GetCompanyProfileAsync(int companyId)
    {
        var company = await _db.Companies.FindAsync(companyId)
            ?? throw new KeyNotFoundException("Company not found.");

        return new CompanyProfileDto
        {
            Id      = company.Id,
            Name    = company.Name,
            Email   = company.Email,
            Phone   = company.Phone,
            Address = company.Address,
            LogoUrl = company.LogoUrl
        };
    }

    public async Task<CompanyProfileDto> UpdateCompanyProfileAsync(int companyId, UpdateCompanyProfileDto dto)
    {
        var company = await _db.Companies.FindAsync(companyId)
            ?? throw new KeyNotFoundException("Company not found.");

        company.Name    = dto.Name.Trim();
        company.Phone   = dto.Phone;
        company.Address = dto.Address;
        await _db.SaveChangesAsync();

        return new CompanyProfileDto
        {
            Id      = company.Id,
            Name    = company.Name,
            Email   = company.Email,
            Phone   = company.Phone,
            Address = company.Address,
            LogoUrl = company.LogoUrl
        };
    }

    public async Task<SalaryRuleDto> GetSalaryRulesAsync(int companyId)
    {
        var rule = await _db.SalaryRules.FirstOrDefaultAsync(r => r.CompanyId == companyId)
            ?? new SalaryRule { CompanyId = companyId };

        return MapToDto(rule);
    }

    public async Task<SalaryRuleDto> UpdateSalaryRulesAsync(int companyId, UpdateSalaryRuleDto dto)
    {
        var rule = await _db.SalaryRules.FirstOrDefaultAsync(r => r.CompanyId == companyId);
        if (rule is null)
        {
            rule = new SalaryRule { CompanyId = companyId };
            _db.SalaryRules.Add(rule);
        }

        rule.PFPercentage            = dto.PFPercentage;
        rule.PFEmployeePercentage    = dto.PFEmployeePercentage;
        rule.ESIPercentage           = dto.ESIPercentage;
        rule.ESIEmployerPercentage   = dto.ESIEmployerPercentage;
        rule.ESISalaryLimit          = dto.ESISalaryLimit;
        rule.WorkingDaysPerMonth     = dto.WorkingDaysPerMonth;
        rule.CasualLeavesPerYear     = dto.CasualLeavesPerYear;
        rule.SickLeavesPerYear       = dto.SickLeavesPerYear;
        rule.EarnedLeavesPerYear     = dto.EarnedLeavesPerYear;
        rule.OvertimeRateMultiplier  = dto.OvertimeRateMultiplier;
        rule.UpdatedAt               = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(rule);
    }

    private static SalaryRuleDto MapToDto(SalaryRule r) => new()
    {
        Id                      = r.Id,
        PFPercentage            = r.PFPercentage,
        PFEmployeePercentage    = r.PFEmployeePercentage,
        ESIPercentage           = r.ESIPercentage,
        ESIEmployerPercentage   = r.ESIEmployerPercentage,
        ESISalaryLimit          = r.ESISalaryLimit,
        WorkingDaysPerMonth     = r.WorkingDaysPerMonth,
        CasualLeavesPerYear     = r.CasualLeavesPerYear,
        SickLeavesPerYear       = r.SickLeavesPerYear,
        EarnedLeavesPerYear     = r.EarnedLeavesPerYear,
        OvertimeRateMultiplier  = r.OvertimeRateMultiplier
    };
}
