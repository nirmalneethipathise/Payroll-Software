using System.ComponentModel.DataAnnotations;
using PayrollAPI.Models;

namespace PayrollAPI.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Designation { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary => BasicSalary + HRA + TransportAllowance + OtherAllowances;
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSC { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }

    [MaxLength(150)]
    public string? Designation { get; set; }

    [Required]
    public DateTime JoinDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required, Range(0, 10000000)]
    public decimal BasicSalary { get; set; }

    [Range(0, 10000000)]
    public decimal HRA { get; set; }

    [Range(0, 10000000)]
    public decimal TransportAllowance { get; set; }

    [Range(0, 10000000)]
    public decimal OtherAllowances { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(30)]
    public string? BankAccountNumber { get; set; }

    [MaxLength(20)]
    public string? BankIFSC { get; set; }
}

public class UpdateEmployeeDto : CreateEmployeeDto
{
    public string? Status { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateDepartmentDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
