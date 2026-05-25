using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayrollAPI.Data;
using PayrollAPI.DTOs.Auth;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;
using PayrollAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PayrollAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower() && u.Company.IsActive);

        if (user is null || !PasswordHelper.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Your account has been deactivated.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
    {
        if (await _db.Companies.AnyAsync(c => c.Email == dto.CompanyEmail.ToLower()))
            throw new InvalidOperationException("A company with this email already exists.");

        if (await _db.Users.AnyAsync(u => u.Email == dto.AdminEmail.ToLower()))
            throw new InvalidOperationException("A user with this email already exists.");

        var company = new Company
        {
            Name  = dto.CompanyName.Trim(),
            Email = dto.CompanyEmail.Trim().ToLower(),
            Phone = dto.Phone
        };
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        // Seed default salary rules
        _db.SalaryRules.Add(new SalaryRule { CompanyId = company.Id });

        var admin = new User
        {
            CompanyId    = company.Id,
            Email        = dto.AdminEmail.Trim().ToLower(),
            PasswordHash = PasswordHelper.Hash(dto.AdminPassword),
            FirstName    = dto.AdminFirstName.Trim(),
            LastName     = dto.AdminLastName.Trim(),
            Role         = UserRole.Admin
        };
        _db.Users.Add(admin);
        await _db.SaveChangesAsync();

        admin.Company = company;
        return BuildAuthResponse(admin);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!PasswordHelper.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = PasswordHelper.Hash(dto.NewPassword);
        await _db.SaveChangesAsync();
    }

    // ── Private helpers ───────────────────────────────────────

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var jwt      = _config.GetSection("JwtSettings");
        var key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds    = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expHours = double.Parse(jwt["ExpirationHours"] ?? "8");
        var expires  = DateTime.UtcNow.AddHours(expHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("companyId", user.CompanyId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:   jwt["Issuer"],
            audience: jwt["Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token       = new JwtSecurityTokenHandler().WriteToken(token),
            Email       = user.Email,
            FullName    = $"{user.FirstName} {user.LastName}",
            Role        = user.Role.ToString(),
            CompanyId   = user.CompanyId,
            CompanyName = user.Company.Name,
            ExpiresAt   = expires
        };
    }
}
