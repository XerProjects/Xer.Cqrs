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
        /// <typeparam name="TQuery">Type of query to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <returns>Result of the dispatched <typeparamref name="TQuery"/> query.</returns>
        public TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : class, IQuery<TResult>
        {
            // Wait Task completion.
            return DispatchAsync<TQuery, TResult>(query).AwaitResult();
        }

        /// <summary>
        /// Dispatch query to the registered query handler asynchronously.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in query handlers.</param>
        /// <returns>Asynchronous task which contains the result of the dispatched <typeparamref name="TQuery"/> query.</returns>
        public Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default(CancellationToken)) where TQuery : class, IQuery<TResult>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            QueryHandlerDelegate<TResult> handleQueryAsyncDelegate = _resolver.ResolveQueryHandler<TQuery, TResult>();

            if (handleQueryAsyncDelegate == null)
            {
                Type queryType = typeof(TQuery);
                throw new NoQueryHandlerResolvedException($"No query handler is registered to handle query of type: {queryType.Name}.", queryType);
            }

            return handleQueryAsyncDelegate.Invoke(query, cancellationToken);
        }
    }
}
