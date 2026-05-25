using PayrollAPI.DTOs.Payroll;

namespace PayrollAPI.Interfaces;

public interface IPayrollService
{
    Task<List<PayrollRunDto>> GetPayrollRunsAsync(int companyId, int? year = null);
    Task<PayrollRunDto> GetPayrollRunByIdAsync(int id, int companyId);
    Task<PayrollRunDto> ProcessPayrollAsync(int companyId, ProcessPayrollDto dto);
    Task<PayrollRunDto> MarkAsPaidAsync(int id, int companyId);

    Task<List<PayslipDto>> GetPayslipsByRunAsync(int payrollRunId, int companyId);
    Task<PayslipDto> GetPayslipByIdAsync(int id, int companyId);
    Task<List<PayslipDto>> GetEmployeePayslipsAsync(int employeeId, int companyId);
    Task<PayslipDto> UpdatePayslipAsync(int id, int companyId, UpdatePayslipDto dto);
    Task<byte[]> GeneratePayslipPdfAsync(int id, int companyId);
}
