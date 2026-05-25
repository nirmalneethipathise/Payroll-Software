using PayrollAPI.DTOs.Common;
using PayrollAPI.DTOs.Employee;

namespace PayrollAPI.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(int companyId, PaginationParams p, int? departmentId = null, string? status = null);
    Task<EmployeeDto> GetByIdAsync(int id, int companyId);
    Task<EmployeeDto> CreateAsync(int companyId, CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(int id, int companyId, UpdateEmployeeDto dto);
    Task DeleteAsync(int id, int companyId);

    Task<List<DepartmentDto>> GetDepartmentsAsync(int companyId);
    Task<DepartmentDto> CreateDepartmentAsync(int companyId, CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateDepartmentAsync(int id, int companyId, CreateDepartmentDto dto);
    Task DeleteDepartmentAsync(int id, int companyId);
}
