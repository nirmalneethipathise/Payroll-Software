using Microsoft.EntityFrameworkCore;
using PayrollAPI.Models;

namespace PayrollAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company>    Companies    { get; set; }
    public DbSet<User>       Users        { get; set; }
    public DbSet<Department> Departments  { get; set; }
    public DbSet<Employee>   Employees    { get; set; }
    public DbSet<Attendance> Attendances  { get; set; }
    public DbSet<Leave>      Leaves       { get; set; }
    public DbSet<PayrollRun> PayrollRuns  { get; set; }
    public DbSet<Payslip>    Payslips     { get; set; }
    public DbSet<SalaryRule> SalaryRules  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Company ───────────────────────────────────────────
        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(c => c.Email).IsUnique();
        });

        // ─── User ──────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => new { u.CompanyId, u.Email }).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();

            e.HasOne(u => u.Company)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(u => u.Employee)
             .WithMany()
             .HasForeignKey(u => u.EmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Department ────────────────────────────────────────
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(d => new { d.CompanyId, d.Name }).IsUnique();

            e.HasOne(d => d.Company)
             .WithMany(c => c.Departments)
             .HasForeignKey(d => d.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Employee ──────────────────────────────────────────
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(emp => emp.Id);
            e.Property(emp => emp.EmployeeCode).HasMaxLength(50).IsRequired();
            e.HasIndex(emp => new { emp.CompanyId, emp.EmployeeCode }).IsUnique();
            e.Property(emp => emp.Email).HasMaxLength(200).IsRequired();
            e.Property(emp => emp.BasicSalary).HasColumnType("decimal(12,2)");
            e.Property(emp => emp.HRA).HasColumnType("decimal(12,2)");
            e.Property(emp => emp.TransportAllowance).HasColumnType("decimal(12,2)");
            e.Property(emp => emp.OtherAllowances).HasColumnType("decimal(12,2)");
            e.Property(emp => emp.Status).HasConversion<string>();

            e.HasOne(emp => emp.Company)
             .WithMany(c => c.Employees)
             .HasForeignKey(emp => emp.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(emp => emp.Department)
             .WithMany(d => d.Employees)
             .HasForeignKey(emp => emp.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Attendance ────────────────────────────────────────
        modelBuilder.Entity<Attendance>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.CompanyId, a.EmployeeId, a.Date }).IsUnique();
            e.Property(a => a.Status).HasConversion<string>();

            e.HasOne(a => a.Company)
             .WithMany(c => c.Attendances)
             .HasForeignKey(a => a.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Employee)
             .WithMany(emp => emp.Attendances)
             .HasForeignKey(a => a.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Leave ─────────────────────────────────────────────
        modelBuilder.Entity<Leave>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.LeaveType).HasConversion<string>();
            e.Property(l => l.Status).HasConversion<string>();

            e.HasOne(l => l.Company)
             .WithMany(c => c.Leaves)
             .HasForeignKey(l => l.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Employee)
             .WithMany(emp => emp.Leaves)
             .HasForeignKey(l => l.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── PayrollRun ────────────────────────────────────────
        modelBuilder.Entity<PayrollRun>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.CompanyId, p.Month, p.Year }).IsUnique();
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.TotalGrossSalary).HasColumnType("decimal(14,2)");
            e.Property(p => p.TotalDeductions).HasColumnType("decimal(14,2)");
            e.Property(p => p.TotalNetSalary).HasColumnType("decimal(14,2)");

            e.HasOne(p => p.Company)
             .WithMany(c => c.PayrollRuns)
             .HasForeignKey(p => p.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Payslip ───────────────────────────────────────────
        modelBuilder.Entity<Payslip>(e =>
        {
            e.HasKey(ps => ps.Id);
            e.HasIndex(ps => new { ps.CompanyId, ps.PayrollRunId, ps.EmployeeId }).IsUnique();

            var decimalCols = new[]
            {
                nameof(Payslip.BasicSalary), nameof(Payslip.HRA),
                nameof(Payslip.TransportAllowance), nameof(Payslip.OtherAllowances),
                nameof(Payslip.Bonus), nameof(Payslip.OvertimePay), nameof(Payslip.GrossSalary),
                nameof(Payslip.PFDeduction), nameof(Payslip.ESIDeduction),
                nameof(Payslip.TaxDeduction), nameof(Payslip.LeaveDeduction),
                nameof(Payslip.OtherDeductions), nameof(Payslip.TotalDeductions),
                nameof(Payslip.NetSalary)
            };
            foreach (var col in decimalCols)
                e.Property(col).HasColumnType("decimal(12,2)");

            e.Property(ps => ps.PaymentStatus).HasConversion<string>();

            e.HasOne(ps => ps.Company)
             .WithMany(c => c.Payslips)
             .HasForeignKey(ps => ps.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ps => ps.PayrollRun)
             .WithMany(pr => pr.Payslips)
             .HasForeignKey(ps => ps.PayrollRunId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ps => ps.Employee)
             .WithMany(emp => emp.Payslips)
             .HasForeignKey(ps => ps.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── SalaryRule ────────────────────────────────────────
        modelBuilder.Entity<SalaryRule>(e =>
        {
            e.HasKey(sr => sr.Id);
            e.HasIndex(sr => sr.CompanyId).IsUnique();

            e.Property(sr => sr.PFPercentage).HasColumnType("decimal(5,2)");
            e.Property(sr => sr.PFEmployeePercentage).HasColumnType("decimal(5,2)");
            e.Property(sr => sr.ESIPercentage).HasColumnType("decimal(5,2)");
            e.Property(sr => sr.ESIEmployerPercentage).HasColumnType("decimal(5,2)");
            e.Property(sr => sr.ESISalaryLimit).HasColumnType("decimal(10,2)");
            e.Property(sr => sr.OvertimeRateMultiplier).HasColumnType("decimal(5,2)");

            e.HasOne(sr => sr.Company)
             .WithMany(c => c.SalaryRules)
             .HasForeignKey(sr => sr.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
