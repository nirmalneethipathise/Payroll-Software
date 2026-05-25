using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Employee;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;

namespace PayrollAPI.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<Employee>> GetPagedAsync(int companyId, PaginationParams p, int? departmentId = null, string? status = null)
    {
        var query = _db.Employees
            .Include(e => e.Department)
            .Where(e => e.CompanyId == companyId);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EmployeeStatus>(status, true, out var parsedStatus))
            query = query.Where(e => e.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var search = p.Search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                e.EmployeeCode.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.FirstName)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return new PagedResult<Employee>
        {
            Items = items,
            TotalCount = total,
            Page = p.Page,
            PageSize = p.PageSize
        };
    }

    public async Task<Employee?> GetByIdAsync(int id, int companyId) =>
        await _db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId);

    public async Task<Employee?> GetByEmailAsync(string email, int companyId) =>
        await _db.Employees
            .FirstOrDefaultAsync(e => e.Email == email && e.CompanyId == companyId);

    public async Task<bool> CodeExistsAsync(string code, int companyId, int? excludeId = null)
    {
        var query = _db.Employees.Where(e => e.EmployeeCode == code && e.CompanyId == companyId);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<int> GetNextEmployeeNumberAsync(int companyId)
    {
        var max = await _db.Employees
            .Where(e => e.CompanyId == companyId)
            .CountAsync();
        return max + 1;
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    public async Task DeleteAsync(Employee employee)
    {
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Employee>> GetAllActiveAsync(int companyId) =>
        await _db.Employees
            .Include(e => e.Department)
            .Where(e => e.CompanyId == companyId && e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.FirstName)
            .ToListAsync();
}
