using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs.Settings;

public class CompanyProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
}

public class UpdateCompanyProfileDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}

public class SalaryRuleDto
{
    public int Id { get; set; }
    public decimal PFPercentage { get; set; }
    public decimal PFEmployeePercentage { get; set; }
    public decimal ESIPercentage { get; set; }
    public decimal ESIEmployerPercentage { get; set; }
    public decimal ESISalaryLimit { get; set; }
    public int WorkingDaysPerMonth { get; set; }
    public int CasualLeavesPerYear { get; set; }
    public int SickLeavesPerYear { get; set; }
    public int EarnedLeavesPerYear { get; set; }
    public decimal OvertimeRateMultiplier { get; set; }
}

public class UpdateSalaryRuleDto
{
    [Range(0, 30)]
    public decimal PFPercentage { get; set; }

    [Range(0, 30)]
    public decimal PFEmployeePercentage { get; set; }

    [Range(0, 10)]
    public decimal ESIPercentage { get; set; }

    [Range(0, 10)]
    public decimal ESIEmployerPercentage { get; set; }

    [Range(0, 100000)]
    public decimal ESISalaryLimit { get; set; }

    [Range(1, 31)]
    public int WorkingDaysPerMonth { get; set; }

    [Range(0, 365)]
    public int CasualLeavesPerYear { get; set; }

    [Range(0, 365)]
    public int SickLeavesPerYear { get; set; }

    [Range(0, 365)]
    public int EarnedLeavesPerYear { get; set; }

    [Range(1, 5)]
    public decimal OvertimeRateMultiplier { get; set; }
}

public class LeaveSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CasualLeaveAllotted { get; set; }
    public int CasualLeaveUsed { get; set; }
    public int SickLeaveAllotted { get; set; }
    public int SickLeaveUsed { get; set; }
    public int EarnedLeaveAllotted { get; set; }
    public int EarnedLeaveUsed { get; set; }
}
