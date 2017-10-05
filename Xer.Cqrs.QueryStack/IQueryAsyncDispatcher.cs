using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryAsyncDispatcher
    {
        /// <summary>
        /// Dispatch query to the registered query handler asynchronously.
        /// </summary>
        /// <typeparam name="TResult">Type of expected query result.</typeparam>
        /// <param name="query">Query to send to registered the query handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancelltaion.</param>
        /// <returns>Task which contains the result of the dispatched query. This can be awaited asynchronously.</returns>
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken));
    }
}
