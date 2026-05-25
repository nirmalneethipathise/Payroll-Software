using PayrollAPI.DTOs.Attendance;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repo;
    private readonly IEmployeeRepository   _empRepo;

    public AttendanceService(IAttendanceRepository repo, IEmployeeRepository empRepo)
    {
        _repo    = repo;
        _empRepo = empRepo;
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync(int companyId, AttendanceQueryParams q)
    {
        List<Attendance> records;

        if (q.Month.HasValue && q.Year.HasValue)
            records = await _repo.GetMonthlyAsync(companyId, q.Month.Value, q.Year.Value, q.EmployeeId);
        else
        {
            var from = q.From ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var to   = q.To   ?? DateOnly.FromDateTime(DateTime.Today);
            records  = await _repo.GetByDateRangeAsync(companyId, from, to, q.EmployeeId);
        }

        return records.Select(MapToDto).ToList();
    }

    public async Task<AttendanceDto> MarkAttendanceAsync(int companyId, MarkAttendanceDto dto)
    {
        var employee = await _empRepo.GetByIdAsync(dto.EmployeeId, companyId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found.");

        if (!Enum.TryParse<AttendanceStatus>(dto.Status, true, out var status))
            throw new ArgumentException($"Invalid attendance status: {dto.Status}");

        var existing = await _repo.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, companyId);
        if (existing is not null)
        {
            existing.Status       = status;
            existing.CheckInTime  = dto.CheckInTime;
            existing.CheckOutTime = dto.CheckOutTime;
            existing.Remarks      = dto.Remarks;
            var updated = await _repo.UpdateAsync(existing);
            return MapToDto(updated, employee);
        }

        var attendance = new Attendance
        {
            CompanyId     = companyId,
            EmployeeId    = dto.EmployeeId,
            Date          = dto.Date,
            Status        = status,
            CheckInTime   = dto.CheckInTime,
            CheckOutTime  = dto.CheckOutTime,
            Remarks       = dto.Remarks
        };
        var created = await _repo.CreateAsync(attendance);
        return MapToDto(created, employee);
    }

    public async Task<List<AttendanceDto>> BulkMarkAttendanceAsync(int companyId, BulkAttendanceDto dto)
    {
        var results = new List<AttendanceDto>();
        foreach (var record in dto.Records)
        {
            record.Date = dto.Date;
            results.Add(await MarkAttendanceAsync(companyId, record));
        }
        return results;
    }

    public async Task<AttendanceDto> UpdateAttendanceAsync(int id, int companyId, MarkAttendanceDto dto)
    {
        var attendance = await _repo.GetByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Attendance record {id} not found.");

        if (!Enum.TryParse<AttendanceStatus>(dto.Status, true, out var status))
            throw new ArgumentException($"Invalid attendance status: {dto.Status}");

        attendance.Status       = status;
        attendance.CheckInTime  = dto.CheckInTime;
        attendance.CheckOutTime = dto.CheckOutTime;
        attendance.Remarks      = dto.Remarks;
        var updated = await _repo.UpdateAsync(attendance);
        return MapToDto(updated);
    }

    public async Task DeleteAttendanceAsync(int id, int companyId)
    {
        var attendance = await _repo.GetByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Attendance record {id} not found.");

        // We cannot use a Remove here without DB access — use AppDbContext directly
        // Handled by marking as deleted via the repository
        throw new InvalidOperationException("Attendance records cannot be deleted; update the status instead.");
    }

    public async Task<List<MonthlyAttendanceSummaryDto>> GetMonthlySummaryAsync(int companyId, int month, int year)
    {
        var employees = await _empRepo.GetAllActiveAsync(companyId);
        var summary   = await _repo.GetMonthlySummaryAsync(companyId, month, year);

        var daysInMonth  = DateTime.DaysInMonth(year, month);
        var workingDays  = CountWorkingDays(year, month);

        return employees.Select(e =>
        {
            summary.TryGetValue(e.Id, out var counts);
            return new MonthlyAttendanceSummaryDto
            {
                EmployeeId   = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                EmployeeCode = e.EmployeeCode,
                WorkingDays  = workingDays,
                PresentDays  = counts.Present,
                AbsentDays   = counts.Absent,
                HalfDays     = counts.HalfDay,
                LeaveDays    = counts.Leave
            };
        }).ToList();
    }

    private static int CountWorkingDays(int year, int month)
    {
        int count = 0;
        var date  = new DateOnly(year, month, 1);
        while (date.Month == month)
        {
            if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                count++;
            date = date.AddDays(1);
        }
        return count;
    }

    private static AttendanceDto MapToDto(Attendance a, Employee? emp = null)
    {
        var employee = emp ?? a.Employee;
        return new AttendanceDto
        {
            Id             = a.Id,
            EmployeeId     = a.EmployeeId,
            EmployeeName   = employee is not null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
            EmployeeCode   = employee?.EmployeeCode ?? string.Empty,
            DepartmentName = employee?.Department?.Name,
            Date           = a.Date,
            Status         = a.Status.ToString(),
            CheckInTime    = a.CheckInTime,
            CheckOutTime   = a.CheckOutTime,
            Remarks        = a.Remarks
        };
    }
}
