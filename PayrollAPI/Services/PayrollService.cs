using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Payroll;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PayrollAPI.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository    _payrollRepo;
    private readonly IAttendanceRepository _attRepo;
    private readonly IEmployeeRepository   _empRepo;
    private readonly AppDbContext          _db;

    public PayrollService(
        IPayrollRepository payrollRepo,
        IAttendanceRepository attRepo,
        IEmployeeRepository empRepo,
        AppDbContext db)
    {
        _payrollRepo = payrollRepo;
        _attRepo     = attRepo;
        _empRepo     = empRepo;
        _db          = db;
    }

    public async Task<List<PayrollRunDto>> GetPayrollRunsAsync(int companyId, int? year = null)
    {
        var runs = await _payrollRepo.GetRunsAsync(companyId, year);
        return runs.Select(MapRunToDto).ToList();
    }

    public async Task<PayrollRunDto> GetPayrollRunByIdAsync(int id, int companyId)
    {
        var run = await _payrollRepo.GetRunByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Payroll run {id} not found.");
        return MapRunToDto(run);
    }

    public async Task<PayrollRunDto> ProcessPayrollAsync(int companyId, ProcessPayrollDto dto)
    {
        var existing = await _payrollRepo.GetRunByMonthYearAsync(companyId, dto.Month, dto.Year);
        if (existing is not null)
            throw new InvalidOperationException($"Payroll for {new DateTime(dto.Year, dto.Month, 1):MMMM yyyy} has already been processed.");

        var rule = await _db.SalaryRules.FirstOrDefaultAsync(r => r.CompanyId == companyId)
            ?? new SalaryRule();

        var employees   = await _empRepo.GetAllActiveAsync(companyId);
        var attSummary  = await _attRepo.GetMonthlySummaryAsync(companyId, dto.Month, dto.Year);
        var workingDays = rule.WorkingDaysPerMonth;

        // Create the run
        var run = new PayrollRun
        {
            CompanyId = companyId,
            Month     = dto.Month,
            Year      = dto.Year,
            Status    = PayrollRunStatus.Processed,
            ProcessedAt = DateTime.UtcNow
        };
        await _payrollRepo.CreateRunAsync(run);

        var payslips = new List<Payslip>();
        foreach (var emp in employees)
        {
            attSummary.TryGetValue(emp.Id, out var att);

            // Salary per day
            var perDay     = (emp.BasicSalary + emp.HRA + emp.TransportAllowance + emp.OtherAllowances) / workingDays;
            var presentDays = att.Present + (att.HalfDay * 0.5m);
            var lossOfPay  = Math.Max(0, workingDays - (int)presentDays - att.Leave) * perDay;

            // Gross
            var gross = emp.BasicSalary + emp.HRA + emp.TransportAllowance + emp.OtherAllowances;

            // PF — on basic salary
            var pfDeduction  = emp.BasicSalary * (rule.PFEmployeePercentage / 100m);

            // ESI — on gross if gross <= limit
            var esiDeduction = gross <= rule.ESISalaryLimit
                ? gross * (rule.ESIPercentage / 100m)
                : 0m;

            var totalDeductions = pfDeduction + esiDeduction + lossOfPay;
            var net             = gross - totalDeductions;

            payslips.Add(new Payslip
            {
                CompanyId          = companyId,
                PayrollRunId       = run.Id,
                EmployeeId         = emp.Id,
                Month              = dto.Month,
                Year               = dto.Year,
                BasicSalary        = emp.BasicSalary,
                HRA                = emp.HRA,
                TransportAllowance = emp.TransportAllowance,
                OtherAllowances    = emp.OtherAllowances,
                GrossSalary        = gross,
                WorkingDays        = workingDays,
                PresentDays        = att.Present,
                AbsentDays         = att.Absent,
                HalfDays           = att.HalfDay,
                LeaveDays          = att.Leave,
                PFDeduction        = Math.Round(pfDeduction, 2),
                ESIDeduction       = Math.Round(esiDeduction, 2),
                LeaveDeduction     = Math.Round(lossOfPay, 2),
                TotalDeductions    = Math.Round(totalDeductions, 2),
                NetSalary          = Math.Round(net, 2)
            });
        }

        await _payrollRepo.BulkCreatePayslipsAsync(payslips);

        // Update run totals
        run.EmployeeCount    = payslips.Count;
        run.TotalGrossSalary = payslips.Sum(p => p.GrossSalary);
        run.TotalDeductions  = payslips.Sum(p => p.TotalDeductions);
        run.TotalNetSalary   = payslips.Sum(p => p.NetSalary);
        await _payrollRepo.UpdateRunAsync(run);

        return MapRunToDto(run);
    }

    public async Task<PayrollRunDto> MarkAsPaidAsync(int id, int companyId)
    {
        var run = await _payrollRepo.GetRunByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Payroll run {id} not found.");

        if (run.Status == PayrollRunStatus.Paid)
            throw new InvalidOperationException("Payroll run is already marked as paid.");

        run.Status = PayrollRunStatus.Paid;
        await _payrollRepo.UpdateRunAsync(run);
        return MapRunToDto(run);
    }

    public async Task<List<PayslipDto>> GetPayslipsByRunAsync(int payrollRunId, int companyId)
    {
        var payslips = await _payrollRepo.GetPayslipsByRunAsync(payrollRunId, companyId);
        return payslips.Select(MapPayslipToDto).ToList();
    }

    public async Task<PayslipDto> GetPayslipByIdAsync(int id, int companyId)
    {
        var payslip = await _payrollRepo.GetPayslipByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Payslip {id} not found.");
        return MapPayslipToDto(payslip);
    }

    public async Task<List<PayslipDto>> GetEmployeePayslipsAsync(int employeeId, int companyId)
    {
        var payslips = await _payrollRepo.GetPayslipsByEmployeeAsync(employeeId, companyId);
        return payslips.Select(MapPayslipToDto).ToList();
    }

    public async Task<PayslipDto> UpdatePayslipAsync(int id, int companyId, UpdatePayslipDto dto)
    {
        var payslip = await _payrollRepo.GetPayslipByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Payslip {id} not found.");

        payslip.Bonus          = dto.Bonus;
        payslip.OvertimePay    = dto.OvertimePay;
        payslip.OtherDeductions= dto.OtherDeductions;
        payslip.GrossSalary    = payslip.BasicSalary + payslip.HRA + payslip.TransportAllowance
                                 + payslip.OtherAllowances + dto.Bonus + dto.OvertimePay;
        payslip.TotalDeductions= payslip.PFDeduction + payslip.ESIDeduction
                                 + payslip.LeaveDeduction + payslip.TaxDeduction + dto.OtherDeductions;
        payslip.NetSalary      = payslip.GrossSalary - payslip.TotalDeductions;

        await _payrollRepo.UpdatePayslipAsync(payslip);
        return MapPayslipToDto(payslip);
    }

    public async Task<byte[]> GeneratePayslipPdfAsync(int id, int companyId)
    {
        var payslip = await _payrollRepo.GetPayslipByIdAsync(id, companyId)
            ?? throw new KeyNotFoundException($"Payslip {id} not found.");

        var dto = MapPayslipToDto(payslip);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(dto.CompanyName).Bold().FontSize(16);
                            c.Item().Text(dto.CompanyAddress ?? string.Empty).FontSize(9);
                        });
                        row.ConstantItem(150).AlignRight().Column(c =>
                        {
                            c.Item().Text("PAYSLIP").Bold().FontSize(14);
                            c.Item().Text(dto.MonthName).FontSize(10);
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    // Employee info
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Employee: {dto.EmployeeName}").Bold();
                            c.Item().Text($"Code: {dto.EmployeeCode}");
                            c.Item().Text($"Designation: {dto.Designation ?? "-"}");
                            c.Item().Text($"Department: {dto.DepartmentName ?? "-"}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Bank: {dto.BankName ?? "-"}");
                            c.Item().Text($"Account: {dto.BankAccountNumber ?? "-"}");
                            c.Item().Text($"Payment: {dto.PaymentStatus}");
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    // Attendance
                    col.Item().PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Working Days: {dto.WorkingDays}");
                        row.RelativeItem().Text($"Present: {dto.PresentDays}");
                        row.RelativeItem().Text($"Absent: {dto.AbsentDays}");
                        row.RelativeItem().Text($"Half Days: {dto.HalfDays}");
                        row.RelativeItem().Text($"Leave Days: {dto.LeaveDays}");
                    });

                    col.Item().LineHorizontal(1);

                    // Earnings & Deductions table
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        // Earnings
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("EARNINGS").Bold().FontSize(11);
                            EarningRow(c, "Basic Salary",         dto.BasicSalary);
                            EarningRow(c, "HRA",                  dto.HRA);
                            EarningRow(c, "Transport Allowance",  dto.TransportAllowance);
                            EarningRow(c, "Other Allowances",     dto.OtherAllowances);
                            EarningRow(c, "Bonus",                dto.Bonus);
                            EarningRow(c, "Overtime",             dto.OvertimePay);
                            c.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Gross Salary").Bold();
                                r.ConstantItem(80).AlignRight().Text($"₹{dto.GrossSalary:N2}").Bold();
                            });
                        });

                        row.ConstantItem(20);

                        // Deductions
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DEDUCTIONS").Bold().FontSize(11);
                            EarningRow(c, "PF",              dto.PFDeduction);
                            EarningRow(c, "ESI",             dto.ESIDeduction);
                            EarningRow(c, "Tax (TDS)",       dto.TaxDeduction);
                            EarningRow(c, "Leave Deduction", dto.LeaveDeduction);
                            EarningRow(c, "Other",           dto.OtherDeductions);
                            c.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Total Deductions").Bold();
                                r.ConstantItem(80).AlignRight().Text($"₹{dto.TotalDeductions:N2}").Bold();
                            });
                        });
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(2);

                    // Net salary
                    col.Item().AlignRight().Text($"NET SALARY: ₹{dto.NetSalary:N2}").Bold().FontSize(14);

                    col.Item().PaddingTop(20).Text("This is a computer-generated payslip and does not require a signature.")
                        .FontSize(8).Italic();
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── Helpers ───────────────────────────────────────────────

    private static void EarningRow(ColumnDescriptor col, string label, decimal value)
    {
        if (value == 0) return;
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label);
            r.ConstantItem(80).AlignRight().Text($"₹{value:N2}");
        });
    }

    private static PayrollRunDto MapRunToDto(PayrollRun r) => new()
    {
        Id               = r.Id,
        Month            = r.Month,
        Year             = r.Year,
        Status           = r.Status.ToString(),
        TotalGrossSalary = r.TotalGrossSalary,
        TotalDeductions  = r.TotalDeductions,
        TotalNetSalary   = r.TotalNetSalary,
        EmployeeCount    = r.EmployeeCount,
        ProcessedAt      = r.ProcessedAt,
        CreatedAt        = r.CreatedAt
    };

    private static PayslipDto MapPayslipToDto(Payslip ps) => new()
    {
        Id                 = ps.Id,
        EmployeeId         = ps.EmployeeId,
        EmployeeName       = ps.Employee is not null ? $"{ps.Employee.FirstName} {ps.Employee.LastName}" : string.Empty,
        EmployeeCode       = ps.Employee?.EmployeeCode ?? string.Empty,
        Designation        = ps.Employee?.Designation,
        DepartmentName     = ps.Employee?.Department?.Name,
        Month              = ps.Month,
        Year               = ps.Year,
        BasicSalary        = ps.BasicSalary,
        HRA                = ps.HRA,
        TransportAllowance = ps.TransportAllowance,
        OtherAllowances    = ps.OtherAllowances,
        Bonus              = ps.Bonus,
        OvertimePay        = ps.OvertimePay,
        GrossSalary        = ps.GrossSalary,
        WorkingDays        = ps.WorkingDays,
        PresentDays        = ps.PresentDays,
        AbsentDays         = ps.AbsentDays,
        HalfDays           = ps.HalfDays,
        LeaveDays          = ps.LeaveDays,
        PFDeduction        = ps.PFDeduction,
        ESIDeduction       = ps.ESIDeduction,
        TaxDeduction       = ps.TaxDeduction,
        LeaveDeduction     = ps.LeaveDeduction,
        OtherDeductions    = ps.OtherDeductions,
        TotalDeductions    = ps.TotalDeductions,
        NetSalary          = ps.NetSalary,
        PaymentStatus      = ps.PaymentStatus.ToString(),
        PaidAt             = ps.PaidAt,
        CompanyName        = ps.Company?.Name ?? string.Empty,
        CompanyAddress     = ps.Company?.Address,
        BankName           = ps.Employee?.BankName,
        BankAccountNumber  = ps.Employee?.BankAccountNumber
    };
}
