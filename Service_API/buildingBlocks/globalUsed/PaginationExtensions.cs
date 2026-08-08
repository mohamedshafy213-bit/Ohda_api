//using Microsoft.EntityFrameworkCore;
//using Service_API.Services.Interfaces;

//namespace Service_API.BuildingBlocks.GlobalUsed;

///// <summary>
///// Extension methods for pagination on IQueryable and IEnumerable
///// </summary>
//public static class PaginationExtensions
//{
//    /// <summary>
//    /// Applies pagination to an IQueryable and returns a PagedResult (database-level pagination)
//    /// </summary>
//    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
//        this IQueryable<T> query,
//        PaginationQuery pagination,
//        CancellationToken cancellationToken = default)
//    {
//        var totalCount = await query.CountAsync(cancellationToken);

//        var items = await query
//            .Skip((pagination.Page - 1) * pagination.PageSize)
//            .Take(pagination.PageSize)
//            .ToListAsync(cancellationToken);

//        return new PagedResult<T>(items, totalCount, pagination.Page, pagination.PageSize);
//    }

//    /// <summary>
//    /// Applies pagination to an IEnumerable and returns a PagedResult (in-memory pagination)
//    /// Use this for combined/merged lists that can't be paginated at database level
//    /// </summary>
//    public static PagedResult<T> ToPagedResult<T>(
//        this IEnumerable<T> source,
//        PaginationQuery pagination)
//    {
//        var enumerable = source as IList<T> ?? source.ToList();
//        var totalCount = enumerable.Count;

//        var items = enumerable
//            .Skip((pagination.Page - 1) * pagination.PageSize)
//            .Take(pagination.PageSize)
//            .ToList();

//        return new PagedResult<T>(items, totalCount, pagination.Page, pagination.PageSize);
//    }

//    /// <summary>
//    /// Applies pagination to an IQueryable with projection and returns a PagedResult
//    /// </summary>
//    public static async Task<PagedResult<TResult>> ToPagedResultAsync<T, TResult>(
//        this IQueryable<T> query,
//        PaginationQuery pagination,
//        Func<T, TResult> selector,
//        CancellationToken cancellationToken = default)
//    {
//        var totalCount = await query.CountAsync(cancellationToken);

//        var items = await query
//            .Skip((pagination.Page - 1) * pagination.PageSize)
//            .Take(pagination.PageSize)
//            .ToListAsync(cancellationToken);

//        var projectedItems = items.Select(selector).ToList();

//        return new PagedResult<TResult>(projectedItems, totalCount, pagination.Page, pagination.PageSize);
//    }
//}
