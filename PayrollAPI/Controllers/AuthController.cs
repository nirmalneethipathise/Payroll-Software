using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs.Auth;
using PayrollAPI.DTOs.Common;
using PayrollAPI.Helpers;
using PayrollAPI.Interfaces;

namespace PayrollAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Register a new company + admin account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyDto dto)
    {
        var result = await _auth.RegisterCompanyAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Company registered successfully."));
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>Change current user password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = ClaimsHelper.GetUserId(User);
        await _auth.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    /// <summary>Decode token claims — useful for frontend bootstrap.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId    = ClaimsHelper.GetUserId(User),
            CompanyId = ClaimsHelper.GetCompanyId(User),
            Email     = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                     ?? User.FindFirst("email")?.Value,
            Role      = ClaimsHelper.GetRole(User)
        });
    }
}
