using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher, IQueryAsyncDispatcher
    {
        private readonly IQueryHandlerProvider _provider;

        public QueryDispatcher(IQueryHandlerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Dispatch queriy to the registered query handler.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <returns>Result of the dispatched query.</returns>
        public TResult Dispatch<TResult>(IQuery<TResult> query)
        {
            // Wait Task completion.
            return DispatchAsync(query).Await();
        }

        /// <summary>
        /// Dispatch query to the registered query handler asynchronously.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancelltaion.</param>
        /// <returns>Task which contains the result of the dispatched query. This can be awaited asynchronously.</returns>
        public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken))
        {
            Type queryType = query.GetType();

            QueryAsyncHandlerDelegate<TResult> handleQueryAsyncDelegate = _provider.GetQueryHandler<TResult>(queryType);

            return handleQueryAsyncDelegate.Invoke(query, cancellationToken);
        }
    }
}
