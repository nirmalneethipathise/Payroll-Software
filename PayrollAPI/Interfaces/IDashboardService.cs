using PayrollAPI.DTOs.Dashboard;

namespace PayrollAPI.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int companyId);
}
