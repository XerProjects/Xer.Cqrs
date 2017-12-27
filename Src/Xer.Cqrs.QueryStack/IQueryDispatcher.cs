namespace Xer.Cqrs.QueryStack
{
    public interface IQueryDispatcher
    {
        /// <summary>
        /// Dispatch query to the registered query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="query">Query to send to the registered query handler.</param>
        /// <returns>Result of the dispatched <typeparamref name="TQuery"/> query.</returns>
        TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : class, IQuery<TResult>;
    }
}