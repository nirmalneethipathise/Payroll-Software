using PayrollAPI.DTOs.Auth;

namespace PayrollAPI.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
