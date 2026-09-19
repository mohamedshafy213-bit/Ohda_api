namespace Entities.Models.Interfaces;

/// <summary>
/// Service to get the current tenant branch context from the authenticated user's claims
/// </summary>
public interface ICurrentBranch
{
    /// <summary>
    /// Current Branch ID extracted exclusively from verified JWT claims.
    /// Returns null if unauthenticated or for SuperAdmin without branch context.
    /// </summary>
    int? BranchId { get; }

    /// <summary>
    /// True if the authenticated user has the SuperAdmin platform role.
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// True if SuperAdmin has provided an explicit audit reason to inspect tenant data.
    /// </summary>
    bool HasSupportAccess { get; }

    /// <summary>
    /// Enforces that a valid BranchId exists. Throws UnauthorizedAccessException if null.
    /// </summary>
    int EnsureBranchId();
}
