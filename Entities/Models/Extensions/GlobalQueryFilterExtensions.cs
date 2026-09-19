using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Entities.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models.Extensions;

/// <summary>
/// Extension methods to apply global query filters for soft delete and multi-tenancy
/// </summary>
public static class GlobalQueryFilterExtensions
{
    private static readonly MethodInfo SetQueryFilterMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(SetQueryFilter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo ResolveBranchIdMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(ResolveBranchId), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo CheckSupportAccessMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(CheckSupportAccess), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    /// Applies global query filters for soft delete (IsDeleted == false) and multi-tenancy (BranchId == currentBranchId)
    /// </summary>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, IHttpContextAccessor? httpContextAccessor)
    {
        if (httpContextAccessor == null)
        {
            ApplySoftDeleteFilter(modelBuilder);
            return;
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(clrType);
            var hasTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);

            if (!hasSoftDelete && !hasTenant) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterExpression = null;

            // Soft delete filter: e.IsDeleted == false
            if (hasSoftDelete)
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                filterExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            }

            // Tenant filter: (hasSupportAccess) OR (resolvedBranchId != 0 && e.BranchId == resolvedBranchId)
            if (hasTenant)
            {
                var branchIdProperty = Expression.Property(parameter, nameof(ITenantEntity.BranchId));

                // Context method calls - EF evaluates dynamically on the context instance
                var resolvedBranchId = Expression.Call(null, ResolveBranchIdMethod, Expression.Constant(httpContextAccessor));
                var resolvedSupportAccess = Expression.Call(null, CheckSupportAccessMethod, Expression.Constant(httpContextAccessor));

                // (resolvedSupportAccess == true) || (resolvedBranchId != 0 && e.BranchId == resolvedBranchId)
                var supportAccessMatches = Expression.IsTrue(resolvedSupportAccess);
                var hasValidBranch = Expression.NotEqual(resolvedBranchId, Expression.Constant(0));
                var branchMatches = Expression.AndAlso(hasValidBranch, Expression.Equal(branchIdProperty, resolvedBranchId));

                var tenantFilter = Expression.OrElse(supportAccessMatches, branchMatches);

                filterExpression = filterExpression == null
                    ? tenantFilter
                    : Expression.AndAlso(filterExpression, tenantFilter);
            }

            if (filterExpression == null) continue;

            var lambda = Expression.Lambda(filterExpression, parameter);
            SetQueryFilterMethod
                .MakeGenericMethod(clrType)
                .Invoke(null, new object[] { modelBuilder, lambda });
        }
    }

    /// <summary>
    /// Overload for backwards compatibility with ITenantService
    /// </summary>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantService? tenantService)
    {
        // Handled via IHttpContextAccessor in ApplyGlobalFilters(ModelBuilder, IHttpContextAccessor)
    }

    /// <summary>
    /// Applies only soft delete filter (IsDeleted == false) to entities implementing ISoftDelete
    /// </summary>
    public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(ISoftDelete).IsAssignableFrom(clrType)) continue;

            var parameter = Expression.Parameter(clrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var filterExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(filterExpression, parameter);

            SetQueryFilterMethod
                .MakeGenericMethod(clrType)
                .Invoke(null, new object[] { modelBuilder, lambda });
        }
    }

    private static void SetQueryFilter<TEntity>(ModelBuilder modelBuilder, LambdaExpression filter)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter((Expression<Func<TEntity, bool>>)filter);
    }

    /// <summary>
    /// Evaluated dynamically per query. Resolves the current user's BranchId from claims.
    /// Returns 0 when no branch context is present (which matches no valid BranchId).
    /// </summary>
    public static int ResolveBranchId(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return 0;

        // 1. Check explicit X-Branch-Id header if supplied
        if (httpContext.Request.Headers.TryGetValue("X-Branch-Id", out var headerValue))
        {
            if (int.TryParse(headerValue.ToString(), out var headerBranchId) && headerBranchId > 0)
                return headerBranchId;
        }

        var user = httpContext.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return 0;

        // 2. Check user's JWT branch claims
        var branchClaim = user.FindFirst("branch_id")?.Value
                       ?? user.FindFirst("BranchId")?.Value;

        if (int.TryParse(branchClaim, out var branchId) && branchId > 0)
            return branchId;

        // 3. Fallback: for any authenticated user without explicit branch claim, default to branch 1 (Main branch)
        return 1;
    }

    /// <summary>
    /// Evaluated dynamically per query. Returns true if tenant isolation should be bypassed:
    /// 1. Non-HTTP contexts (background jobs, startup seeding, design-time migrations).
    /// 2. Request explicitly flagged with IgnoreTenantFilter in HttpContext.Items.
    /// 3. Authenticated SuperAdmin (platform-wide oversight).
    /// </summary>
    public static bool CheckSupportAccess(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return true; // Non-HTTP background / startup context

        if (httpContext.Items.TryGetValue("IgnoreTenantFilter", out var ignore) && ignore is true)
            return true;

        var user = httpContext.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return false;

        var isSuperAdmin = user.IsInRole("SuperAdmin")
            || string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.FindFirst("role")?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || user.HasClaim(c => (c.Type == ClaimTypes.Role || c.Type == "role") && string.Equals(c.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase));

        if (isSuperAdmin)
            return true;

        if (httpContext.Request.Headers.TryGetValue("X-Support-Access", out var supportHeaderValue))
        {
            return string.Equals(supportHeaderValue.ToString(), "true", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}