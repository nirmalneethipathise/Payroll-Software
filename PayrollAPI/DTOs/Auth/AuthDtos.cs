using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs.Auth;

public class LoginDto
{
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterCompanyDto
{
    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string CompanyEmail { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required, MaxLength(100)]
    public string AdminFirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string AdminLastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string AdminEmail { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string AdminPassword { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
