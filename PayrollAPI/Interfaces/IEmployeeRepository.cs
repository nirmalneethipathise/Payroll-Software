using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Employee;
using PayrollAPI.Models;

namespace PayrollAPI.Interfaces;

public interface IEmployeeRepository
{
    Task<PagedResult<Employee>> GetPagedAsync(int companyId, PaginationParams p, int? departmentId = null, string? status = null);
    Task<Employee?> GetByIdAsync(int id, int companyId);
    Task<Employee?> GetByEmailAsync(string email, int companyId);
    Task<bool> CodeExistsAsync(string code, int companyId, int? excludeId = null);
    Task<int> GetNextEmployeeNumberAsync(int companyId);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task DeleteAsync(Employee employee);
    Task<List<Employee>> GetAllActiveAsync(int companyId);
}
