using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Employee;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _svc;

    public EmployeesController(IEmployeeService svc) => _svc = svc;

    // ─── Employees ─────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams p,
        [FromQuery] int? departmentId, [FromQuery] string? status)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetEmployeesAsync(companyId, p, departmentId, status);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetByIdAsync(id, companyId);
        return Ok(ApiResponse<EmployeeDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.CreateAsync(companyId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<EmployeeDto>.Ok(result, "Employee created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdateAsync(id, companyId, dto);
        return Ok(ApiResponse<EmployeeDto>.Ok(result, "Employee updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        await _svc.DeleteAsync(id, companyId);
        return Ok(ApiResponse.Ok("Employee deactivated."));
    }

    // ─── Departments ───────────────────────────────────────────

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetDepartmentsAsync(companyId);
        return Ok(ApiResponse<List<DepartmentDto>>.Ok(result));
    }

    [HttpPost("departments")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.CreateDepartmentAsync(companyId, dto);
        return Ok(ApiResponse<DepartmentDto>.Ok(result, "Department created."));
    }

    [HttpPut("departments/{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] CreateDepartmentDto dto)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.UpdateDepartmentAsync(id, companyId, dto);
        return Ok(ApiResponse<DepartmentDto>.Ok(result, "Department updated."));
    }

    [HttpDelete("departments/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        await _svc.DeleteDepartmentAsync(id, companyId);
        return Ok(ApiResponse.Ok("Department deleted."));
    }
}
