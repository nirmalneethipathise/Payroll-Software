using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs.Payroll;

public class PayrollRunDto
{
    public int Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public string Status { get; set; } = string.Empty;
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProcessPayrollDto
{
    [Required, Range(1, 12)]
    public int Month { get; set; }

    [Required, Range(2000, 2100)]
    public int Year { get; set; }
}

public class PayslipDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? DepartmentName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

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
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }

    // Company info (for PDF)
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyAddress { get; set; }

    // Bank
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
}

public class UpdatePayslipDto
{
    [Range(0, 10000000)]
    public decimal Bonus { get; set; }

    [Range(0, 10000000)]
    public decimal OvertimePay { get; set; }

    [Range(0, 10000000)]
    public decimal OtherDeductions { get; set; }
}
