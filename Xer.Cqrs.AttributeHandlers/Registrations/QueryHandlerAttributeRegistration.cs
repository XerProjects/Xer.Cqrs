using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.AttributeHandlers.Registrations
{
    public class QueryHandlerAttributeRegistration : IAttributedHandlerRegistration, IQueryHandlerProvider
    {
        #region Declarations

        private static readonly MethodInfo NonGenericRegisterAsyncQueryHandlerMethod = typeof(QueryHandlerAttributeRegistration).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(registerQueryHandlerMethod));

        private readonly QueryHandlerCache _queryHandlerDelegatesByQueryType = new QueryHandlerCache();

        #endregion Declarations

        #region IQueryHandlerProvider Implementation

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
                throw new NotSupportedException($"No query handler is registered to handle query of type: { queryType.Name }");
            }

            return handleQueryDelegate;
        }

        #endregion IQueryHandlerProvider Implementation

        #region IAttributedHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register all methods of the instance that are marked with [QueryHandler].
        /// In order to be registered successfully, method should:
        /// - Request for the specific query to be handled as a parameter.
        /// - (Optional) Request for a CancellationToken as a parameter to listen for cancellation from Dispatcher.
        /// - Return a Task object whose generic argument matches the query's expected result type.
        /// </summary>
        /// <param name="commandHandler">Object which contains methods marked with [QueryHandler].</param>
        public void RegisterAttributedMethods<TAttributed>(Func<TAttributed> attributedHandlerFactory)
        {
            // Get all public methods marked with CommandHandler attribute.
            Type attributedHandlerType = typeof(TAttributed);
            IEnumerable<QueryHandlerMethod> queryHandlerMethods = GetQueryHandlerMethods(attributedHandlerType);

            foreach (QueryHandlerMethod queryHandlerMethod in queryHandlerMethods)
            {
                MethodInfo genericRegisterQueryHandlerMethod = NonGenericRegisterAsyncQueryHandlerMethod.MakeGenericMethod(attributedHandlerType, queryHandlerMethod.QueryType, queryHandlerMethod.QueryReturnType);

                genericRegisterQueryHandlerMethod.Invoke(this, new object[]
                {
                    attributedHandlerFactory, queryHandlerMethod
                });
            }
        }

        #endregion IAttributedHandlerFactoryRegistration Implementation

        #region Functions

        private static IEnumerable<QueryHandlerMethod> GetQueryHandlerMethods(Type queryHandlerType)
        {
            List<QueryHandlerMethod> queryHandlerMethods = new List<QueryHandlerMethod>();

            foreach(MethodInfo methodInfo in queryHandlerType.GetRuntimeMethods())
            {
                if (!methodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(QueryHandlerAttribute)))
                {
                    // Method not marked with [QueryHandler]. Skip.
                    continue;
                }

                queryHandlerMethods.Add(QueryHandlerMethod.Create(methodInfo));
            }

            return queryHandlerMethods;
        }

        private void registerQueryHandlerMethod<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);

            QueryAsyncHandlerDelegate<TResult> handleQueryDelegate;
            if (_queryHandlerDelegatesByQueryType.TryGetValue(specificQueryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query handler registered for {specificQueryType.Name} query.");
            }

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = queryHandlerMethod.CreateDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);

            _queryHandlerDelegatesByQueryType.Add(specificQueryType, newHandleQueryDelegate);
        }

        #endregion Functions

        #region Inner Cache Class

        private class QueryHandlerCache
        {
            public IDictionary<Type, object> _storage = new Dictionary<Type, object>();

            public void Add<TResult>(Type queryType, QueryAsyncHandlerDelegate<TResult> queryHandlerDelegate)
            {
                _storage.Add(queryType, queryHandlerDelegate);
            }

            public bool TryGetValue<TResult>(Type queryType, out QueryAsyncHandlerDelegate<TResult> queryHandlerDelegate)
            {
                queryHandlerDelegate = default(QueryAsyncHandlerDelegate<TResult>);

                object value;
                if (_storage.TryGetValue(queryType, out value))
                {
                    queryHandlerDelegate = (QueryAsyncHandlerDelegate<TResult>)value;
                    return true;
                }

                return false;
            }
        }

        #endregion Inner Cache Class
    }
}
