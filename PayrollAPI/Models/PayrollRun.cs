namespace PayrollAPI.Models;

public class PayrollRun
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
    public ICollection<Payslip> Payslips { get; set; } = [];
}

public enum PayrollRunStatus
{
    Draft     = 1,
    Processed = 2,
    Paid      = 3
}
