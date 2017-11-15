using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerResolver
    {
        /// <summary>
        /// Get the registered query handler delegate which handles the query of the specified type.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <returns>Instance of <see cref="QueryHandlerDelegate{TResult}"/> which executes the query handler processing.</returns>
        QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>;
    }

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    /// <typeparam name="TResult">The type of query's result.</typeparam>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Optional cancellation token to support cancellation.<param>
    /// <returns>Asynchronous task which contains the result of the dispatched query.</returns>
    public delegate Task<TResult> QueryHandlerDelegate<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken));
}
