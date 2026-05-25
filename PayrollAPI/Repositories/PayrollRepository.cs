using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly AppDbContext _db;

    public PayrollRepository(AppDbContext db) => _db = db;

    // ─── PayrollRun ────────────────────────────────────────────

    public async Task<PayrollRun?> GetRunByIdAsync(int id, int companyId) =>
        await _db.PayrollRuns
            .Include(r => r.Payslips)
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

    public async Task<PayrollRun?> GetRunByMonthYearAsync(int companyId, int month, int year) =>
        await _db.PayrollRuns
            .FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Month == month && r.Year == year);

    public async Task<List<PayrollRun>> GetRunsAsync(int companyId, int? year = null)
    {
        var query = _db.PayrollRuns.Where(r => r.CompanyId == companyId);
        if (year.HasValue)
            query = query.Where(r => r.Year == year.Value);
        return await query.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToListAsync();
    }

    public async Task<PayrollRun> CreateRunAsync(PayrollRun run)
    {
        _db.PayrollRuns.Add(run);
        await _db.SaveChangesAsync();
        return run;
    }

    public async Task<PayrollRun> UpdateRunAsync(PayrollRun run)
    {
        run.UpdatedAt = DateTime.UtcNow;
        _db.PayrollRuns.Update(run);
        await _db.SaveChangesAsync();
        return run;
    }

    // ─── Payslips ──────────────────────────────────────────────

    public async Task<Payslip?> GetPayslipByIdAsync(int id, int companyId) =>
        await _db.Payslips
            .Include(ps => ps.Employee).ThenInclude(e => e.Department)
            .Include(ps => ps.Company)
            .Include(ps => ps.PayrollRun)
            .FirstOrDefaultAsync(ps => ps.Id == id && ps.CompanyId == companyId);

    public async Task<Payslip?> GetPayslipByEmployeeAndRunAsync(int employeeId, int payrollRunId, int companyId) =>
        await _db.Payslips
            .FirstOrDefaultAsync(ps => ps.EmployeeId == employeeId && ps.PayrollRunId == payrollRunId && ps.CompanyId == companyId);

    public async Task<List<Payslip>> GetPayslipsByRunAsync(int payrollRunId, int companyId) =>
        await _db.Payslips
            .Include(ps => ps.Employee).ThenInclude(e => e.Department)
            .Where(ps => ps.PayrollRunId == payrollRunId && ps.CompanyId == companyId)
            .OrderBy(ps => ps.Employee.FirstName)
            .ToListAsync();

    public async Task<List<Payslip>> GetPayslipsByEmployeeAsync(int employeeId, int companyId) =>
        await _db.Payslips
            .Include(ps => ps.PayrollRun)
            .Where(ps => ps.EmployeeId == employeeId && ps.CompanyId == companyId)
            .OrderByDescending(ps => ps.Year).ThenByDescending(ps => ps.Month)
            .ToListAsync();

    public async Task<List<Payslip>> BulkCreatePayslipsAsync(List<Payslip> payslips)
    {
        _db.Payslips.AddRange(payslips);
        await _db.SaveChangesAsync();
        return payslips;
    }

    public async Task<Payslip> UpdatePayslipAsync(Payslip payslip)
    {
        _db.Payslips.Update(payslip);
        await _db.SaveChangesAsync();
        return payslip;
    }
}
