using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Dashboard;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;

    public DashboardController(IDashboardService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var companyId = ClaimsHelper.GetCompanyId(User);
        var result    = await _svc.GetDashboardAsync(companyId);
        return Ok(ApiResponse<DashboardDto>.Ok(result));
    }
}
