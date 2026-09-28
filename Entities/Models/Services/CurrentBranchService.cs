using System;
using System.Security.Claims;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Entities.Models.Services;

public class CurrentBranchService : ICurrentTenant, ICurrentBranch, ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentBranchService>? _logger;
    private int? _explicitBranchId;
    private int? _explicitUserId;
    private bool? _explicitIsSuperAdmin;
    private bool _ignoreTenantFilter;
    private bool _initialized;

    public CurrentBranchService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentBranchService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null)
        {
            // Non-HTTP context without explicit SetTenant call -> 0 (no access)
            _explicitBranchId ??= 0;
            return;
        }

        var user = httpContext.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            _explicitBranchId = 0; // Unauthenticated -> no branch data
            return;
        }

        // 1. Determine if SuperAdmin
        bool isSuperAdmin = user.IsInRole("SuperAdmin")
            || string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.FindFirst("role")?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || user.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role") && string.Equals(c.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase));

        _explicitIsSuperAdmin = isSuperAdmin;

        // 2. Resolve UserId
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
        {
            _explicitUserId = uid;
        }

        // 3. Resolve BranchId strictly from verified JWT claims
        var branchClaim = user.FindFirst("branch_id")?.Value
                       ?? user.FindFirst("BranchId")?.Value;

        if (int.TryParse(branchClaim, out var claimBranchId) && claimBranchId > 0)
        {
            _explicitBranchId = claimBranchId;
        }

        // 4. Allow X-Branch-Id override ONLY for SuperAdmin (with audit logging)
        if (httpContext.Request.Headers.TryGetValue("X-Branch-Id", out var headerVal))
        {
            if (int.TryParse(headerVal.ToString(), out var overrideBranchId) && overrideBranchId > 0)
            {
                if (isSuperAdmin)
                {
                    _logger?.LogInformation("[SUPERADMIN_BRANCH_OVERRIDE] SuperAdmin {UserId} overrode branch context to {BranchId}", _explicitUserId, overrideBranchId);
                    _explicitBranchId = overrideBranchId;
                }
                else
                {
                    _logger?.LogWarning("[SECURITY_ALERT] Non-SuperAdmin user {UserId} attempted unauthorized X-Branch-Id override to {BranchId}", _explicitUserId, overrideBranchId);
                }
            }
        }

        // 5. Authenticated regular user without branch claim gets 0 (no access, never default to branch 1)
        if (!_explicitBranchId.HasValue)
        {
            _explicitBranchId = isSuperAdmin ? null : 0;
        }
    }

    public int? BranchId
    {
        get
        {
            EnsureInitialized();
            return _explicitBranchId;
        }
        set
        {
            _explicitBranchId = value;
            _initialized = true;
        }
    }

    public int? UserId
    {
        get
        {
            EnsureInitialized();
            return _explicitUserId;
        }
        set
        {
            _explicitUserId = value;
            _initialized = true;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            EnsureInitialized();
            return _explicitIsSuperAdmin ?? false;
        }
        set
        {
            _explicitIsSuperAdmin = value;
            _initialized = true;
        }
    }

    public bool IgnoreTenantFilter
    {
        get => _ignoreTenantFilter;
        set => _ignoreTenantFilter = value;
    }

    public bool HasSupportAccess => IsSuperAdmin;

    public void SetTenant(int? branchId, int? userId = null, bool isSuperAdmin = false)
    {
        _explicitBranchId = branchId;
        _explicitUserId = userId;
        _explicitIsSuperAdmin = isSuperAdmin;
        _initialized = true;
    }

    public int EnsureBranchId()
    {
        var id = BranchId;
        if (!id.HasValue || id.Value <= 0)
        {
            throw new UnauthorizedAccessException("Current operation requires a valid Branch context.");
        }
        return id.Value;
    }

    // ITenantService backward compatibility
    public int? GetCurrentUniversityId() => BranchId;
    public bool ShouldApplyTenantFilter() => !IgnoreTenantFilter && !IsSuperAdmin && BranchId.HasValue && BranchId.Value > 0;
}
