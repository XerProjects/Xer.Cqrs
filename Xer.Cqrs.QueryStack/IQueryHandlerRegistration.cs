using System;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerRegistration
    {
        /// <summary>
        /// Register query async handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Asynchronous handler which can process the query.</param>
        void Register<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : class, IQuery<TResult>;

        /// <summary>
        /// Register query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Synchronous handler which can process the query.</param>
        void Register<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : class, IQuery<TResult>;
    }
}
