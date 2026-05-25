using PayrollAPI.DTOs.Attendance;

namespace PayrollAPI.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceDto>> GetAttendanceAsync(int companyId, AttendanceQueryParams query);
    Task<AttendanceDto> MarkAttendanceAsync(int companyId, MarkAttendanceDto dto);
    Task<List<AttendanceDto>> BulkMarkAttendanceAsync(int companyId, BulkAttendanceDto dto);
    Task<AttendanceDto> UpdateAttendanceAsync(int id, int companyId, MarkAttendanceDto dto);
    Task DeleteAttendanceAsync(int id, int companyId);
    Task<List<MonthlyAttendanceSummaryDto>> GetMonthlySummaryAsync(int companyId, int month, int year);
}
