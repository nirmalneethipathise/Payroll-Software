using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _db;

    public AttendanceRepository(AppDbContext db) => _db = db;

    public async Task<Attendance?> GetByIdAsync(int id, int companyId) =>
        await _db.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == companyId);

    public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date, int companyId) =>
        await _db.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date && a.CompanyId == companyId);

    public async Task<List<Attendance>> GetByDateRangeAsync(int companyId, DateOnly from, DateOnly to, int? employeeId = null)
    {
        var query = _db.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Where(a => a.CompanyId == companyId && a.Date >= from && a.Date <= to);

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        return await query.OrderBy(a => a.Date).ThenBy(a => a.Employee.FirstName).ToListAsync();
    }

    public async Task<List<Attendance>> GetMonthlyAsync(int companyId, int month, int year, int? employeeId = null)
    {
        var from = new DateOnly(year, month, 1);
        var to   = from.AddMonths(1).AddDays(-1);
        return await GetByDateRangeAsync(companyId, from, to, employeeId);
    }

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        _db.Attendances.Add(attendance);
        await _db.SaveChangesAsync();
        return attendance;
    }

    public async Task<Attendance> UpdateAsync(Attendance attendance)
    {
        attendance.UpdatedAt = DateTime.UtcNow;
        _db.Attendances.Update(attendance);
        await _db.SaveChangesAsync();
        return attendance;
    }

    public async Task BulkUpsertAsync(List<Attendance> records)
    {
        foreach (var record in records)
        {
            var existing = await GetByEmployeeAndDateAsync(record.EmployeeId, record.Date, record.CompanyId);
            if (existing is null)
                _db.Attendances.Add(record);
            else
            {
                existing.Status       = record.Status;
                existing.CheckInTime  = record.CheckInTime;
                existing.CheckOutTime = record.CheckOutTime;
                existing.Remarks      = record.Remarks;
                existing.UpdatedAt    = DateTime.UtcNow;
                _db.Attendances.Update(existing);
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, (int Present, int Absent, int HalfDay, int Leave)>> GetMonthlySummaryAsync(
        int companyId, int month, int year)
    {
        var from = new DateOnly(year, month, 1);
        var to   = from.AddMonths(1).AddDays(-1);

        var records = await _db.Attendances
            .Where(a => a.CompanyId == companyId && a.Date >= from && a.Date <= to)
            .ToListAsync();

        return records
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => (
                    Present:  g.Count(a => a.Status == AttendanceStatus.Present),
                    Absent:   g.Count(a => a.Status == AttendanceStatus.Absent),
                    HalfDay:  g.Count(a => a.Status == AttendanceStatus.HalfDay),
                    Leave:    g.Count(a => a.Status == AttendanceStatus.Leave)
                )
            );
    }
}
