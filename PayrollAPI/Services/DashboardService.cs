using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Dashboard;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync(int companyId)
    {
        var today        = DateOnly.FromDateTime(DateTime.Today);
        var currentMonth = DateTime.Today.Month;
        var currentYear  = DateTime.Today.Year;

        // Employee counts
        var totalEmployees  = await _db.Employees.CountAsync(e => e.CompanyId == companyId);
        var activeEmployees = await _db.Employees.CountAsync(e => e.CompanyId == companyId && e.Status == Models.EmployeeStatus.Active);
        var totalDepts      = await _db.Departments.CountAsync(d => d.CompanyId == companyId);

        // Today's attendance
        var todayAttendance = await _db.Attendances
            .Where(a => a.CompanyId == companyId && a.Date == today)
            .ToListAsync();

        var todayPresent = todayAttendance.Count(a => a.Status == Models.AttendanceStatus.Present);
        var todayAbsent  = todayAttendance.Count(a => a.Status == Models.AttendanceStatus.Absent);
        var todayLeave   = todayAttendance.Count(a => a.Status == Models.AttendanceStatus.Leave);

        // Pending leave requests
        var pendingLeaves = await _db.Leaves
            .CountAsync(l => l.CompanyId == companyId && l.Status == Models.LeaveStatus.Pending);

        // Current month payroll
        var currentPayroll = await _db.PayrollRuns
            .Where(r => r.CompanyId == companyId && r.Month == currentMonth && r.Year == currentYear)
            .Select(r => r.TotalNetSalary)
            .FirstOrDefaultAsync();

        // Last month payroll
        var lastMonth  = currentMonth == 1 ? 12 : currentMonth - 1;
        var lastYear   = currentMonth == 1 ? currentYear - 1 : currentYear;
        var lastPayroll = await _db.PayrollRuns
            .Where(r => r.CompanyId == companyId && r.Month == lastMonth && r.Year == lastYear)
            .Select(r => r.TotalNetSalary)
            .FirstOrDefaultAsync();

        // Payroll trend (last 6 months)
        var trend = await _db.PayrollRuns
            .Where(r => r.CompanyId == companyId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Take(6)
            .Select(r => new MonthlyPayrollTrendDto
            {
                Month         = new DateTime(r.Year, r.Month, 1).ToString("MMM yy"),
                TotalPayroll  = r.TotalNetSalary,
                EmployeeCount = r.EmployeeCount
            })
            .ToListAsync();

        // Department head counts
        var deptCounts = await _db.Departments
            .Where(d => d.CompanyId == companyId)
            .Select(d => new DepartmentHeadCountDto
            {
                Department = d.Name,
                Count      = d.Employees.Count(e => e.Status == Models.EmployeeStatus.Active)
            })
            .ToListAsync();

        // Recent payroll runs
        var recentRuns = await _db.PayrollRuns
            .Where(r => r.CompanyId == companyId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Take(5)
            .Select(r => new RecentPayrollDto
            {
                Id            = r.Id,
                MonthYear     = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy"),
                NetSalary     = r.TotalNetSalary,
                EmployeeCount = r.EmployeeCount,
                Status        = r.Status.ToString()
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalEmployees        = totalEmployees,
            ActiveEmployees       = activeEmployees,
            TotalDepartments      = totalDepts,
            CurrentMonthPayroll   = currentPayroll,
            LastMonthPayroll      = lastPayroll,
            TodayPresent          = todayPresent,
            TodayAbsent           = todayAbsent,
            TodayOnLeave          = todayLeave,
            PendingLeaveRequests  = pendingLeaves,
            PayrollTrend          = trend,
            DepartmentHeadCounts  = deptCounts,
            RecentPayrolls        = recentRuns
        };
    }
}
