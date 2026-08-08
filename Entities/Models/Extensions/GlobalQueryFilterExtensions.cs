using System.Linq.Expressions;
using System.Reflection;
using Entities.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models.Extensions;

/// <summary>
/// Extension methods to apply global query filters for soft delete and multi-tenancy
/// </summary>
public static class GlobalQueryFilterExtensions
{
    // Cache reflection lookups � these never change at runtime
    private static readonly MethodInfo SetQueryFilterMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(SetQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo ResolveUniversityIdMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(ResolveUniversityId), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Applies global query filters for soft delete (IsDeleted) and multi-tenancy (UniversityId)
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="tenantService">The tenant service to get current university ID</param>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantService? tenantService)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(clrType);
            var hasTenant = typeof(ITenantEntity).IsAssignableFrom(clrType) && tenantService != null;

            if (!hasSoftDelete && !hasTenant) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterExpression = null;

            // Soft delete filter: e.IsDeleted == false
            if (hasSoftDelete)
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                filterExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            }

            // Tenant filter: e.UniversityId == ResolveUniversityId(tenantService)
            // Only applied when a tenant context actually exists at query time
            if (hasTenant)
            {
                var universityIdProperty = Expression.Property(parameter, nameof(ITenantEntity.UniversityId));
                var tenantServiceConstant = Expression.Constant(tenantService);

                // Single call to ResolveUniversityId � result is reused in both checks below
                var resolvedUniversityId = Expression.Call(null, ResolveUniversityIdMethod, tenantServiceConstant);

                // (universityId == 0) OR (e.UniversityId == universityId)
                // Bypasses filtering entirely when no tenant context exists (universityId == 0)
                var noTenantContext = Expression.Equal(resolvedUniversityId, Expression.Constant(0));
                var tenantMatches = Expression.Equal(universityIdProperty, resolvedUniversityId);
                var tenantFilter = Expression.OrElse(noTenantContext, tenantMatches);

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

    /// <summary>
    /// Helper method to apply query filter using generic type
    /// </summary>
    private static void SetQueryFilter<TEntity>(ModelBuilder modelBuilder, LambdaExpression filter)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter((Expression<Func<TEntity, bool>>)filter);
    }

    /// <summary>
    /// Resolves the nullable university ID to a plain int.
    /// Returns 0 when no tenant context exists � since no real UniversityId
    /// should ever be 0, this effectively bypasses tenant filtering.
    /// </summary>
    private static int ResolveUniversityId(ITenantService tenantService)
    {
        return tenantService.GetCurrentUniversityId() ?? 0;
    }
}