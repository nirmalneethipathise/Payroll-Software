namespace PayrollAPI.Models;

public class Payslip
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Bonus { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal GrossSalary { get; set; }

    // Attendance
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }

    // Deductions
    public decimal PFDeduction { get; set; }
    public decimal ESIDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal LeaveDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    public decimal NetSalary { get; set; }

    public PayslipPaymentStatus PaymentStatus { get; set; } = PayslipPaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
    public PayrollRun PayrollRun { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

public enum PayslipPaymentStatus
{
    Pending = 1,
    Paid    = 2
}
