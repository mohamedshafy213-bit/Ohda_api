using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Entities.Models.ClassHelper;

namespace Entities.Models.Services;

/// <summary>
/// Service to get the current tenant (university) ID from the HTTP context
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current university ID from the authenticated user's claims
    /// </summary>
    public int? GetCurrentUniversityId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Try to get UniversityId from claims
        var universityIdClaim = httpContext.User?.FindFirst(CustomClaims.UniversityId)?.Value
                             ?? httpContext.User?.FindFirst(CustomClaims.UniversityIdClaimLower)?.Value
                             ?? httpContext.User?.FindFirst(CustomClaims.UniversityIdClaim)?.Value;

        if (int.TryParse(universityIdClaim, out var universityId))
        {
            return universityId;
        }

        // Try to get from header (for cases where it's passed as a header)
        if (httpContext.Request.Headers.TryGetValue("X-University-Id", out var headerValue))
        {
            if (int.TryParse(headerValue, out var headerUniversityId))
            {
                return headerUniversityId;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if tenant filtering should be applied
    /// </summary>
    public bool ShouldApplyTenantFilter()
    {
        return GetCurrentUniversityId().HasValue;
    }
}



