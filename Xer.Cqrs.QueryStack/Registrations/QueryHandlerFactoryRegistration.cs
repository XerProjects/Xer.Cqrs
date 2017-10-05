using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Registrations
{
    public partial class QueryHandlerFactoryRegistration : IQueryHandlerProvider, IQueryHandlerFactoryRegistration
    {
        #region Declarations

        private readonly QueryHandlerCache _queryHandlerDelegatesByQueryType = new QueryHandlerCache();

        #endregion Declarations

        #region IQueryAsyncHandlerProvider Implementation

        /// <summary>
        /// Get the registered query handler delegate to handle the query of the specified type.
        /// </summary>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        public QueryAsyncHandlerDelegate<TResult> GetQueryHandler<TResult>(Type queryType)
        {
            QueryAsyncHandlerDelegate<TResult> handleQueryDelegate;

            if (!_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new QueryNotHandledException($"No query handler is registered to handle query of type: { queryType.Name }");
            }

            return handleQueryDelegate;
        }

        #endregion IQueryAsyncHandlerProvider Implementation

        #region IQueryHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register query handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Synchronous handler which can process the query.</param>
        public void Register<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : IQuery<TResult>
        {
            Type queryType = typeof(TQuery);

            QueryAsyncHandlerDelegate<TResult> handleQueryDelegate;

            if (_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query async handler registered for {queryType.Name}.");
            }

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                IQueryHandler<TQuery, TResult> queryHandlerInstance = queryHandlerFactory.Invoke();

                if (queryHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                TResult result = queryHandlerInstance.Handle((TQuery)q);

                return Task.FromResult(result);
            };

            _queryHandlerDelegatesByQueryType.Add(queryType, newHandleQueryDelegate);
        }

        /// <summary>
        /// Register query async handler.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Query's expected result.</typeparam>
        /// <param name="queryHandlerFactory">Asynchronous handler which can process the query.</param>
        public void Register<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryHandlerFactory) where TQuery : IQuery<TResult>
        {
            Type queryType = typeof(TQuery);

            QueryAsyncHandlerDelegate<TResult> handleQueryDelegate;

            if (_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query async handler registered for {queryType.Name}.");
            }

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                IQueryAsyncHandler<TQuery, TResult> queryHandlerInstance = queryHandlerFactory.Invoke();

                if (queryHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                return queryHandlerInstance.HandleAsync((TQuery)q, ct);
            };

            _queryHandlerDelegatesByQueryType.Add(queryType, newHandleQueryDelegate);
        }

        #endregion IQueryHandlerFactoryRegistration Implementation
        
        #region Inner Cache Class

        private class QueryHandlerCache
        {
            public IDictionary<Type, object> _storage = new Dictionary<Type, object>();

            public void Add<TResult>(Type queryType, QueryAsyncHandlerDelegate<TResult> handleQueryDelegate)
            {
                _storage.Add(queryType, handleQueryDelegate);
            }

            public bool TryGetValue<TResult>(Type queryType, out QueryAsyncHandlerDelegate<TResult> handleQueryDelegate)
            {
                handleQueryDelegate = default(QueryAsyncHandlerDelegate<TResult>);

                object value;
                if (_storage.TryGetValue(queryType, out value))
                {
                    handleQueryDelegate = (QueryAsyncHandlerDelegate<TResult>)value;
                    return true;
                }

                return false;
            }
        }

        #endregion Inner Cache Class
    }
}
