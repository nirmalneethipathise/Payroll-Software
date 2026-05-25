using PayrollAPI.Models;

namespace PayrollAPI.Interfaces;

public interface IPayrollRepository
{
    Task<PayrollRun?> GetRunByIdAsync(int id, int companyId);
    Task<PayrollRun?> GetRunByMonthYearAsync(int companyId, int month, int year);
    Task<List<PayrollRun>> GetRunsAsync(int companyId, int? year = null);
    Task<PayrollRun> CreateRunAsync(PayrollRun run);
    Task<PayrollRun> UpdateRunAsync(PayrollRun run);

    Task<Payslip?> GetPayslipByIdAsync(int id, int companyId);
    Task<Payslip?> GetPayslipByEmployeeAndRunAsync(int employeeId, int payrollRunId, int companyId);
    Task<List<Payslip>> GetPayslipsByRunAsync(int payrollRunId, int companyId);
    Task<List<Payslip>> GetPayslipsByEmployeeAsync(int employeeId, int companyId);
    Task<List<Payslip>> BulkCreatePayslipsAsync(List<Payslip> payslips);
    Task<Payslip> UpdatePayslipAsync(Payslip payslip);
}
