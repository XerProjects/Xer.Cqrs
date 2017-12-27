using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryAsyncDispatcher
    {
        /// <summary>
        /// Dispatch query to the registered query handler asynchronously.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in query handlers.</param>
        /// <returns>Asynchronous task which contains the result of the dispatched <typeparamref name="TQuery"/> query.</returns>
        Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default(CancellationToken)) where TQuery : class, IQuery<TResult>;
    }
}