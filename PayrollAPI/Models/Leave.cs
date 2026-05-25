namespace PayrollAPI.Models;

public class Leave
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Days { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

public enum LeaveType
{
    CasualLeave  = 1,
    SickLeave    = 2,
    EarnedLeave  = 3,
    Maternity    = 4,
    Paternity    = 5,
    Unpaid       = 6
}

public enum LeaveStatus
{
    Pending  = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled= 4
}
