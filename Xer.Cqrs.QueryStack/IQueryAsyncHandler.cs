using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryAsyncHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handle and process the query asynchronously.
        /// </summary>
        /// <param name="query">Query to handle and process.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which contains the result of the dispatched query. This can be awaited asynchronously.</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default(CancellationToken));
    }
}
