using System.Security.Claims;

namespace PayrollAPI.Helpers;

/// <summary>Reads well-known claims from a ClaimsPrincipal without magic strings.</summary>
public static class ClaimsHelper
{
    public static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? throw new UnauthorizedAccessException("User ID claim missing."));

    public static int GetCompanyId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirst("companyId")?.Value
               ?? throw new UnauthorizedAccessException("CompanyId claim missing."));

    public static string GetRole(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
