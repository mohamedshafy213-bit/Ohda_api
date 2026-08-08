namespace Contracts.Responses
{
    /// <summary>
    /// ParentResonseModel Class use as a response for Repositorties Function that return true or false
    /// plus list of objects returend from the database
    /// </summary>
    public class ListOfObjectsResponseModel<T> : SingleObjectResponseModel
    {
        /// <summary>
        /// listOfObjects returened from the database to the end point
        /// </summary>
        public IEnumerable<T>? Objects { get; set; }
    }
}
