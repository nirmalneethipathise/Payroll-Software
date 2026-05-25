namespace PayrollAPI.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public decimal CurrentMonthPayroll { get; set; }
    public decimal LastMonthPayroll { get; set; }
    public int TodayPresent { get; set; }
    public int TodayAbsent { get; set; }
    public int TodayOnLeave { get; set; }
    public int PendingLeaveRequests { get; set; }
    public List<MonthlyPayrollTrendDto> PayrollTrend { get; set; } = [];
    public List<DepartmentHeadCountDto> DepartmentHeadCounts { get; set; } = [];
    public List<RecentPayrollDto> RecentPayrolls { get; set; } = [];
}

public class MonthlyPayrollTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal TotalPayroll { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentHeadCountDto
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentPayrollDto
{
    public int Id { get; set; }
    public string MonthYear { get; set; } = string.Empty;
    public decimal NetSalary { get; set; }
    public int EmployeeCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
