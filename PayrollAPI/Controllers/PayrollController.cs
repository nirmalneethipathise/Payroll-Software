using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Payroll;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _svc;

    public PayrollController(IPayrollService svc) => _svc = svc;

    // ─── Payroll Runs ──────────────────────────────────────────

    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns([FromQuery] int? year)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetPayrollRunsAsync(companyId, year);
        return Ok(ApiResponse<List<PayrollRunDto>>.Ok(result));
    }

    [HttpGet("runs/{id:int}")]
    public async Task<IActionResult> GetRun(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetPayrollRunByIdAsync(id, companyId);
        return Ok(ApiResponse<PayrollRunDto>.Ok(result));
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ProcessPayroll([FromBody] ProcessPayrollDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.ProcessPayrollAsync(companyId, dto);
        return Ok(ApiResponse<PayrollRunDto>.Ok(result, "Payroll processed successfully."));
    }

    [HttpPost("runs/{id:int}/mark-paid")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.MarkAsPaidAsync(id, companyId);
        return Ok(ApiResponse<PayrollRunDto>.Ok(result, "Payroll marked as paid."));
    }

    // ─── Payslips ──────────────────────────────────────────────

    [HttpGet("runs/{runId:int}/payslips")]
    public async Task<IActionResult> GetPayslipsByRun(int runId)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetPayslipsByRunAsync(runId, companyId);
        return Ok(ApiResponse<List<PayslipDto>>.Ok(result));
    }

    [HttpGet("payslips/{id:int}")]
    public async Task<IActionResult> GetPayslip(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetPayslipByIdAsync(id, companyId);
        return Ok(ApiResponse<PayslipDto>.Ok(result));
    }

    [HttpGet("payslips/employee/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeePayslips(int employeeId)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetEmployeePayslipsAsync(employeeId, companyId);
        return Ok(ApiResponse<List<PayslipDto>>.Ok(result));
    }

    [HttpPut("payslips/{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdatePayslip(int id, [FromBody] UpdatePayslipDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdatePayslipAsync(id, companyId, dto);
        return Ok(ApiResponse<PayslipDto>.Ok(result, "Payslip updated."));
    }

    [HttpGet("payslips/{id:int}/pdf")]
    public async Task<IActionResult> DownloadPayslip(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var pdfBytes  = await _svc.GeneratePayslipPdfAsync(id, companyId);
        return File(pdfBytes, "application/pdf", $"payslip-{id}.pdf");
    }
}
