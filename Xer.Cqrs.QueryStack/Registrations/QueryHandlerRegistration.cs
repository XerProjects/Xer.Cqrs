using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Registrations
{
    public partial class QueryHandlerRegistration : IQueryHandlerResolver, IQueryHandlerRegistration
    {
        #region Declarations

        private readonly QueryHandlerDelegateStore _queryHandlerDelegatesByQueryType = new QueryHandlerDelegateStore();

        #endregion Declarations

        #region IQueryHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Synchronous handler which can process the query.</param>
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
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Asynchronous handler which can process the query.</param>
        public void Register<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : class, IQuery<TResult>
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

        #endregion IQueryHandlerFactoryRegistration Implementation

        #region IQueryHandlerResolver Implementation

        /// <summary>
        /// Get the registered query handler delegate to handle the query of the specified type.
        /// </summary>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
        {
            Type queryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;

            if (!_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new QueryNotHandledException($"No query handler is registered to handle query of type: { queryType.Name }.");
            }

            return handleQueryDelegate;
        }

        #endregion IQueryHandlerResolver Implementation
    }
}
