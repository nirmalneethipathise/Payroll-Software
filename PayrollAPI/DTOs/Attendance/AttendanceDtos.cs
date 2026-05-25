using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs.Attendance;

public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public DateOnly Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class MarkAttendanceDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class BulkAttendanceDto
{
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public List<MarkAttendanceDto> Records { get; set; } = [];
}

public class MonthlyAttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }
    public double AttendancePercent => WorkingDays > 0
        ? Math.Round((double)PresentDays / WorkingDays * 100, 1) : 0;
}

public class AttendanceQueryParams
{
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}
