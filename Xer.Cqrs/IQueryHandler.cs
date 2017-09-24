namespace Xer.Cqrs
{
    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handle and process the query asynchronously.
        /// </summary>
        /// <param name="query">Query to handle and process.</param>
        /// <returns>Result of the query.</returns>
        TResult Handle(TQuery query);
    }
}
