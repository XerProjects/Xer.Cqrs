using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Registrations
{
    public class QueryHandlerRegistration : IQueryHandlerResolver, IQueryHandlerRegistration
    {
        #region Declarations

        private readonly QueryHandlerDelegateStore _queryHandlerDelegatesByQueryType = new QueryHandlerDelegateStore();

        #endregion Declarations

        #region IQueryHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="queryHandlerFactory">Factory which will provide an instance of a query handler that handles the specified <typeparamref name="TQuery"/> query.</param>
        public void Register<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : class, IQuery<TResult>
        {
            if (queryHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(queryHandlerFactory));
            }

            Type queryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;

            if (_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query handler registered for {queryType.Name}.");
            }

            QueryHandlerDelegate<TResult> newHandleQueryDelegate = QueryHandlerDelegateBuilder.FromFactory(queryHandlerFactory);

            _queryHandlerDelegatesByQueryType.Add(queryType, newHandleQueryDelegate);
        }

        /// <summary>
        /// Register query async handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <param name="queryAsyncHandlerFactory">Factory which will provide an instance of a query handler that handles the specified <typeparamref name="TQuery"/> query.</param>
        public void Register<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryAsyncHandlerFactory) where TQuery : class, IQuery<TResult>
        {
            if (queryAsyncHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(queryAsyncHandlerFactory));
            }

            Type queryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;

            if (_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query handler registered for {queryType.Name}.");
            }

            QueryHandlerDelegate<TResult> newHandleQueryDelegate = QueryHandlerDelegateBuilder.FromFactory(queryAsyncHandlerFactory);

            _queryHandlerDelegatesByQueryType.Add(queryType, newHandleQueryDelegate);
        }

        #endregion IQueryHandlerFactoryRegistration Implementation

        #region IQueryHandlerResolver Implementation

        /// <summary>
        /// Get the registered query handler delegate which handles the query of the specified type.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <returns>Instance of <see cref="QueryHandlerDelegate{TResult}"/> which executes the query handler processing.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
        {
            Type queryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;

            if (!_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new NoQueryHandlerResolvedException($"No query handler is registered to handle query of type: { queryType.Name }.", queryType);
            }

            return handleQueryDelegate;
        }

        #endregion IQueryHandlerResolver Implementation
    }
}
