using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Attendance;
using PayrollAPI.DTOs.Common;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _svc;

    public AttendanceController(IAttendanceService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAttendance([FromQuery] AttendanceQueryParams q)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetAttendanceAsync(companyId, q);
        return Ok(ApiResponse<List<AttendanceDto>>.Ok(result));
    }

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year)
    {
        if (month < 1 || month > 12) return BadRequest(ApiResponse.Fail("Invalid month."));

        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetMonthlySummaryAsync(companyId, month, year);
        return Ok(ApiResponse<List<MonthlyAttendanceSummaryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.MarkAttendanceAsync(companyId, dto);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "Attendance marked."));
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> BulkMarkAttendance([FromBody] BulkAttendanceDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.BulkMarkAttendanceAsync(companyId, dto);
        return Ok(ApiResponse<List<AttendanceDto>>.Ok(result, $"{result.Count} records saved."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] MarkAttendanceDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdateAttendanceAsync(id, companyId, dto);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "Attendance updated."));
    }
}
