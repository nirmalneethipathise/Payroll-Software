using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Employee;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;

    public EmployeeService(IEmployeeRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db   = db;
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(int companyId, PaginationParams p, int? departmentId = null, string? status = null)
    {
        var paged = await _repo.GetPagedAsync(companyId, p, departmentId, status);
        return new PagedResult<EmployeeDto>
        {
            Items      = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(int id, int companyId)
    {
        var employee = await _repo.GetByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");
        return MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(int companyId, CreateEmployeeDto dto)
    {
        if (await _db.Employees.AnyAsync(e => e.Email == dto.Email.ToLower() && e.CompanyId == companyId))
            throw new InvalidOperationException("An employee with this email already exists.");

        string code;
        if (!string.IsNullOrWhiteSpace(dto.EmployeeCode))
        {
            if (await _repo.CodeExistsAsync(dto.EmployeeCode, companyId))
                throw new InvalidOperationException($"Employee code '{dto.EmployeeCode}' already exists.");
            code = dto.EmployeeCode;
        }
        else
        {
            var num = await _repo.GetNextEmployeeNumberAsync(companyId);
            code = $"EMP{num:D4}";
        }

        var employee = new Employee
        {
            CompanyId          = companyId,
            DepartmentId       = dto.DepartmentId,
            EmployeeCode       = code,
            FirstName          = dto.FirstName.Trim(),
            LastName           = dto.LastName.Trim(),
            Email              = dto.Email.Trim().ToLower(),
            Phone              = dto.Phone,
            Designation        = dto.Designation,
            JoinDate           = dto.JoinDate,
            DateOfBirth        = dto.DateOfBirth,
            Gender             = dto.Gender,
            Address            = dto.Address,
            BasicSalary        = dto.BasicSalary,
            HRA                = dto.HRA,
            TransportAllowance = dto.TransportAllowance,
            OtherAllowances    = dto.OtherAllowances,
            BankName           = dto.BankName,
            BankAccountNumber  = dto.BankAccountNumber,
            BankIFSC           = dto.BankIFSC
        };

        var created = await _repo.CreateAsync(employee);
        return MapToDto(await _repo.GetByIdAsync(created.Id, companyId) ?? created);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, int companyId, UpdateEmployeeDto dto)
    {
        var employee = await _repo.GetByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        // Email uniqueness check (exclude current)
        if (await _db.Employees.AnyAsync(e => e.Email == dto.Email.ToLower() && e.CompanyId == companyId && e.Id != id))
            throw new InvalidOperationException("Another employee with this email already exists.");

        employee.FirstName          = dto.FirstName.Trim();
        employee.LastName           = dto.LastName.Trim();
        employee.Email              = dto.Email.Trim().ToLower();
        employee.Phone              = dto.Phone;
        employee.DepartmentId       = dto.DepartmentId;
        employee.Designation        = dto.Designation;
        employee.JoinDate           = dto.JoinDate;
        employee.DateOfBirth        = dto.DateOfBirth;
        employee.Gender             = dto.Gender;
        employee.Address            = dto.Address;
        employee.BasicSalary        = dto.BasicSalary;
        employee.HRA                = dto.HRA;
        employee.TransportAllowance = dto.TransportAllowance;
        employee.OtherAllowances    = dto.OtherAllowances;
        employee.BankName           = dto.BankName;
        employee.BankAccountNumber  = dto.BankAccountNumber;
        employee.BankIFSC           = dto.BankIFSC;

        if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<EmployeeStatus>(dto.Status, true, out var status))
            employee.Status = status;

        await _repo.UpdateAsync(employee);
        return MapToDto(employee);
    }

    public async Task DeleteAsync(int id, int companyId)
    {
        var employee = await _repo.GetByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        // Soft-delete: mark as terminated instead of hard delete to preserve payroll history
        employee.Status = EmployeeStatus.Terminated;
        await _repo.UpdateAsync(employee);
    }

    // ─── Department methods ────────────────────────────────────

    public async Task<List<DepartmentDto>> GetDepartmentsAsync(int companyId) =>
        await _db.Departments
            .Where(d => d.CompanyId == companyId)
            .Select(d => new DepartmentDto
            {
                Id            = d.Id,
                Name          = d.Name,
                Description   = d.Description,
                EmployeeCount = d.Employees.Count(e => e.Status == EmployeeStatus.Active)
            })
            .OrderBy(d => d.Name)
            .ToListAsync();

    public async Task<DepartmentDto> CreateDepartmentAsync(int companyId, CreateDepartmentDto dto)
    {
        if (await _db.Departments.AnyAsync(d => d.Name == dto.Name.Trim() && d.CompanyId == companyId))
            throw new InvalidOperationException($"Department '{dto.Name}' already exists.");

        var dept = new Department { CompanyId = companyId, Name = dto.Name.Trim(), Description = dto.Description };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description };
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(int id, int companyId, CreateDepartmentDto dto)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId)
            ?? throw new KeyNotFoundException($"Department {id} not found.");

        dept.Name        = dto.Name.Trim();
        dept.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description };
    }

    public async Task DeleteDepartmentAsync(int id, int companyId)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId)
            ?? throw new KeyNotFoundException($"Department {id} not found.");

        if (await _db.Employees.AnyAsync(e => e.DepartmentId == id && e.Status == EmployeeStatus.Active))
            throw new InvalidOperationException("Cannot delete department with active employees.");

        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync();
    }

    // ─── Mapper ────────────────────────────────────────────────

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id                 = e.Id,
        EmployeeCode       = e.EmployeeCode,
        FirstName          = e.FirstName,
        LastName           = e.LastName,
        Email              = e.Email,
        Phone              = e.Phone,
        Designation        = e.Designation,
        DepartmentId       = e.DepartmentId,
        DepartmentName     = e.Department?.Name,
        JoinDate           = e.JoinDate,
        DateOfBirth        = e.DateOfBirth,
        Gender             = e.Gender,
        Address            = e.Address,
        BasicSalary        = e.BasicSalary,
        HRA                = e.HRA,
        TransportAllowance = e.TransportAllowance,
        OtherAllowances    = e.OtherAllowances,
        BankName           = e.BankName,
        BankAccountNumber  = e.BankAccountNumber,
        BankIFSC           = e.BankIFSC,
        Status             = e.Status.ToString(),
        CreatedAt          = e.CreatedAt
    };
}
