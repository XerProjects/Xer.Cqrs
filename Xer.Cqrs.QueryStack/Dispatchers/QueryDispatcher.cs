using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher, IQueryAsyncDispatcher
    {
        private readonly IQueryHandlerResolver _resolver;

        public QueryDispatcher(IQueryHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Dispatch query to the registered query handler.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <returns>Result of the dispatched query.</returns>
        public TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            // Wait Task completion.
            return DispatchAsync<TQuery, TResult>(query).Await();
        }

        /// <summary>
        /// Dispatch query to the registered query handler asynchronously.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancelltaion.</param>
        /// <returns>Task which contains the result of the dispatched query. This can be awaited asynchronously.</returns>
        public Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default(CancellationToken)) where TQuery : IQuery<TResult>
        {
            QueryHandlerDelegate<TResult> handleQueryAsyncDelegate = _resolver.ResolveQueryHandler<TQuery, TResult>();

            return handleQueryAsyncDelegate.Invoke(query, cancellationToken);
        }
    }
}
