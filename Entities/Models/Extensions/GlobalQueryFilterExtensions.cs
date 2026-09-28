using System;
using System.Linq.Expressions;
using System.Reflection;
using Entities.Models.Databases;
using Entities.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models.Extensions;

/// <summary>
/// Configures EF Core global query filters for Soft Delete (IsDeleted == false)
/// and Multi-Tenancy (BranchId == context.BranchFilterId) bound directly to the RepositoryContext instance.
/// </summary>
public static class GlobalQueryFilterExtensions
{
    private static readonly MethodInfo SetQueryFilterMethod = typeof(GlobalQueryFilterExtensions)
        .GetMethod(nameof(SetQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Applies clean, context-bound query filters to all eligible entity types:
    /// 1. ISoftDelete only: e => !e.IsDeleted
    /// 2. ITenantEntity only: e => e.BranchId == context.BranchFilterId
    /// 3. Both: e => !e.IsDeleted && e.BranchId == context.BranchFilterId
    /// </summary>
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, RepositoryContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            bool hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(clrType);
            bool hasTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);

            if (!hasSoftDelete && !hasTenant) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterExpression = null;

            // 1. Soft Delete: !e.IsDeleted
            if (hasSoftDelete)
            {
                var isDeletedProp = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                filterExpression = Expression.Equal(isDeletedProp, Expression.Constant(false));
            }

            // 2. Multi-Tenant: e.BranchId == context.BranchFilterId
            if (hasTenant)
            {
                var branchIdProp = Expression.Property(parameter, nameof(ITenantEntity.BranchId));
                var contextExpr = Expression.Constant(context);
                var branchFilterIdProp = Expression.Property(contextExpr, nameof(RepositoryContext.BranchFilterId));
                var tenantExpression = Expression.Equal(branchIdProp, branchFilterIdProp);

                filterExpression = filterExpression == null
                    ? tenantExpression
                    : Expression.AndAlso(filterExpression, tenantExpression);
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
            var isDeletedProp = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var filterExpression = Expression.Equal(isDeletedProp, Expression.Constant(false));
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
}