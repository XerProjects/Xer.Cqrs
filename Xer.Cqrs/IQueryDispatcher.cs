namespace Xer.Cqrs
{
    public interface IQueryDispatcher
    {
        /// <summary>
        /// Dispatch queriy to the registered query handler.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <returns>Result of the dispatched query.</returns>
        TResult Dispatch<TResult>(IQuery<TResult> query);
    }
}
