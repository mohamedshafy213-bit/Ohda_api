namespace Contracts.Pagging
{
    public static class PaggingExtention
    {
        public static PagedResult<T> GetPaged<T>(this IEnumerable<T> query, int page, int pagesize) where T : class
        {
            var result = new PagedResult<T>();
            result.CurrentPage = page;
            result.PageSize = pagesize;
            result.TotalCount = query.Count();
            var pagecount = (double)result.TotalCount / pagesize;
            result.PageCount = (int)Math.Ceiling(pagecount);

            var skip = (page - 1) * pagesize;
            result.Results = query.Skip(skip).Take(pagesize).ToList();
            return result;
        }

    }
}
