using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Settings;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _svc;

    public SettingsController(ISettingsService svc) => _svc = svc;

    [HttpGet("company")]
    public async Task<IActionResult> GetCompany()
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetCompanyProfileAsync(companyId);
        return Ok(ApiResponse<CompanyProfileDto>.Ok(result));
    }

    [HttpPut("company")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyProfileDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdateCompanyProfileAsync(companyId, dto);
        return Ok(ApiResponse<CompanyProfileDto>.Ok(result, "Company profile updated."));
    }

    [HttpGet("salary-rules")]
    public async Task<IActionResult> GetSalaryRules()
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetSalaryRulesAsync(companyId);
        return Ok(ApiResponse<SalaryRuleDto>.Ok(result));
    }

    [HttpPut("salary-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSalaryRules([FromBody] UpdateSalaryRuleDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdateSalaryRulesAsync(companyId, dto);
        return Ok(ApiResponse<SalaryRuleDto>.Ok(result, "Salary rules updated."));
    }
}
