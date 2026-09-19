using System.Security.Claims;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Entities.Models.Services;

public class CurrentBranchService : ICurrentBranch, ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentBranchService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? BranchId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
                return null;

            var branchClaim = user.FindFirst("branch_id")?.Value
                           ?? user.FindFirst("BranchId")?.Value;

            if (int.TryParse(branchClaim, out var branchId))
                return branchId;

            return null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
                return false;

            return user.IsInRole("SuperAdmin") 
                || string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(user.FindFirst("role")?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool HasSupportAccess
    {
        get
        {
            if (!IsSuperAdmin) return false;

            var supportHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Support-Access"].ToString();
            return string.Equals(supportHeader, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public int EnsureBranchId()
    {
        var branchId = BranchId;
        if (!branchId.HasValue)
        {
            throw new UnauthorizedAccessException("Current operation requires a valid Branch context.");
        }
        return branchId.Value;
    }

    // ITenantService implementation for backwards compatibility
    public int? GetCurrentUniversityId() => BranchId;
    public bool ShouldApplyTenantFilter() => BranchId.HasValue && !HasSupportAccess;
}
