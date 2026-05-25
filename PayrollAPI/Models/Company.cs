namespace PayrollAPI.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Employee> Employees { get; set; } = [];
    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<Attendance> Attendances { get; set; } = [];
    public ICollection<Leave> Leaves { get; set; } = [];
    public ICollection<PayrollRun> PayrollRuns { get; set; } = [];
    public ICollection<Payslip> Payslips { get; set; } = [];
    public ICollection<SalaryRule> SalaryRules { get; set; } = [];
}
