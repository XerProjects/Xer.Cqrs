using System;

namespace Xer.Cqrs.QueryStack
{
    public interface IQueryHandlerRegistration
    {
        /// <summary>
        /// Register query async handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="queryAsyncHandlerFactory">Factory which will provide an instance of a query handler that handles the specified <typeparamref name="TQuery"/> query.</param>
        void Register<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryAsyncHandlerFactory) where TQuery : class, IQuery<TResult>;

        /// <summary>
        /// Register query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="queryHandlerFactory">Factory which will provide an instance of a query handler that handles the specified <typeparamref name="TQuery"/> query.</param>
        void Register<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : class, IQuery<TResult>;
    }
}
