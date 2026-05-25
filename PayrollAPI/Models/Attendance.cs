namespace PayrollAPI.Models;

public class Attendance
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

public enum AttendanceStatus
{
    Present  = 1,
    Absent   = 2,
    HalfDay  = 3,
    Leave    = 4,
    Holiday  = 5,
    WeekOff  = 6
}
