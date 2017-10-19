using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerResolver
    {
        /// <summary>
        /// Get the registered asynchronous delegate to handle the query of the specified type.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult>;
    }

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <returns>Query result.</returns>
    public delegate Task<TResult> QueryHandlerDelegate<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken));
}
