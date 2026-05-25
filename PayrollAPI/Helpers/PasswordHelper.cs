using BCrypt.Net;

namespace PayrollAPI.Helpers;

public static class PasswordHelper
{
    public static string Hash(string password) =>
        BCrypt.HashPassword(password, workFactor: 12);

    public static bool Verify(string password, string hash) =>
        BCrypt.Verify(password, hash);
}
