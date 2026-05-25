using PayrollAPI.Models;

namespace PayrollAPI.Interfaces;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(int id, int companyId);
    Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date, int companyId);
    Task<List<Attendance>> GetByDateRangeAsync(int companyId, DateOnly from, DateOnly to, int? employeeId = null);
    Task<List<Attendance>> GetMonthlyAsync(int companyId, int month, int year, int? employeeId = null);
    Task<Attendance> CreateAsync(Attendance attendance);
    Task<Attendance> UpdateAsync(Attendance attendance);
    Task BulkUpsertAsync(List<Attendance> records);
    Task<Dictionary<int, (int Present, int Absent, int HalfDay, int Leave)>> GetMonthlySummaryAsync(int companyId, int month, int year);
}
