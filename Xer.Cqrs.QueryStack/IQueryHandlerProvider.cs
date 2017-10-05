using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerProvider
    {
        /// <summary>
        /// Get the registered asynchronous delegate to handle the query of the specified type.
        /// </summary>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        QueryAsyncHandlerDelegate<TResult> GetQueryHandler<TResult>(Type queryType);
    }

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <returns>Query result.</returns>
    public delegate Task<TResult> QueryAsyncHandlerDelegate<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken));
}
