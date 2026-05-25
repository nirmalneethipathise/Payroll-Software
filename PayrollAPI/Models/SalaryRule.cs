namespace PayrollAPI.Models;

public class SalaryRule
{
    public int Id { get; set; }
    public int CompanyId { get; set; }

    // PF (Provident Fund)
    public decimal PFPercentage { get; set; } = 12m;
    public decimal PFEmployeePercentage { get; set; } = 12m;

    // ESI (Employee State Insurance)
    public decimal ESIPercentage { get; set; } = 1.75m;
    public decimal ESIEmployerPercentage { get; set; } = 4.75m;
    public decimal ESISalaryLimit { get; set; } = 21000m;

    // Working Days
    public int WorkingDaysPerMonth { get; set; } = 26;

    // Leave Rules
    public int CasualLeavesPerYear { get; set; } = 12;
    public int SickLeavesPerYear { get; set; } = 12;
    public int EarnedLeavesPerYear { get; set; } = 15;

    // Overtime
    public decimal OvertimeRateMultiplier { get; set; } = 2m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company Company { get; set; } = null!;
}
