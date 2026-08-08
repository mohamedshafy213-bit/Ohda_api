using Entities.Models.Tables;
using Microsoft.AspNetCore.Identity;

namespace Service_API.Helpers;

public static class PasswordHasherHelper
{
    private static readonly PasswordHasher<User> _hasher = new();

    public static string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public static bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
