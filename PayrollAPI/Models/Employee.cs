namespace PayrollAPI.Models;

public class Employee
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Designation { get; set; }

    // Salary breakdown
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }

    // Bank details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSC { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = [];
    public ICollection<Leave> Leaves { get; set; } = [];
    public ICollection<Payslip> Payslips { get; set; } = [];
}

public enum EmployeeStatus
{
    Active     = 1,
    Inactive   = 2,
    Terminated = 3
}
