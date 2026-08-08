namespace Contracts.Pagging
{
    public class PagedResult<T> : PagedResultBase where T : class
    {
        public IList<T> Results { get; set; }
        /// <summary>
        /// PagedResult Constractor
        /// </summary>

        public PagedResult()
        {
            Results = new List<T>();
        }
    }
}
