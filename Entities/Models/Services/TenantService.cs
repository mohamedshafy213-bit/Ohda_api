using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Entities.Models.Services;

/// <summary>
/// Backward-compatible adapter service to get current tenant (branch) ID from HTTP context
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentUniversityId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        var branchClaim = httpContext.User?.FindFirst("branch_id")?.Value
                       ?? httpContext.User?.FindFirst("BranchId")?.Value;

        if (int.TryParse(branchClaim, out var branchId))
        {
            return branchId;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Branch-Id", out var headerValue))
        {
            if (int.TryParse(headerValue, out var headerBranchId))
            {
                return headerBranchId;
            }
        }

        return null;
    }

    public bool ShouldApplyTenantFilter()
    {
        return GetCurrentUniversityId().HasValue;
    }
}
