using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xer.Cqrs.QueryStack.Registrations
{
    public class QueryHandlerAttributeRegistration : IQueryHandlerAttributeRegistration, IQueryHandlerResolver
    {
        #region Declarations

        private static readonly MethodInfo RegisterQueryHandlerOpenGenericMethodInfo = typeof(QueryHandlerAttributeRegistration).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(registerQueryHandlerMethod));

        private readonly QueryHandlerDelegateStore _queryHandlerDelegatesByQueryType = new QueryHandlerDelegateStore();

        #endregion Declarations

        #region IQueryHandlerAttributeRegistration Implementation

        /// <summary>
        /// Register all methods of the instance that are marked with [QueryHandler].
        /// In order to be registered successfully, method should:
        /// - Request for the specific query to be handled as a parameter.
        /// - (Optional) Request for a CancellationToken as a parameter to listen for cancellation from Dispatcher.
        /// - Return a Task object whose generic argument matches the query's expected result type.
        /// </summary>
        /// <param name="attributedHandlerFactory">Object which contains methods marked with [QueryHandler].</param>
        public void RegisterQueryHandlerAttributes<TAttributed>(Func<TAttributed> attributedHandlerFactory)
        {
            // Get all public methods marked with CommandHandler attribute.
            Type attributedHandlerType = typeof(TAttributed);
            IEnumerable<QueryHandlerAttributeMethod> queryHandlerMethods = getQueryHandlerMethods(attributedHandlerType);

            foreach (QueryHandlerAttributeMethod queryHandlerMethod in queryHandlerMethods)
            {
                MethodInfo registerQueryHandlerGenericMethodInfo = RegisterQueryHandlerOpenGenericMethodInfo.MakeGenericMethod(
                    attributedHandlerType, 
                    queryHandlerMethod.QueryType, 
                    queryHandlerMethod.QueryReturnType);

                registerQueryHandlerGenericMethodInfo.Invoke(this, new object[]
                {
                    attributedHandlerFactory, queryHandlerMethod
                });
            }
        }

        #endregion IQueryHandlerAttributeRegistration Implementation

        #region IQueryHandlerResolver Implementation

        /// <summary>
        /// Get the registered query handler delegate to handle the query of the specified type.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult>
        {
            Type queryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;

            if (!_queryHandlerDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new QueryNotHandledException($"No query handler is registered to handle query of type: { queryType.Name }");
            }

            return handleQueryDelegate;
        }

        #endregion IQueryHandlerResolver Implementation

        #region Functions

        private void registerQueryHandlerMethod<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, QueryHandlerAttributeMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);

            QueryHandlerDelegate<TResult> handleQueryDelegate;
            if (_queryHandlerDelegatesByQueryType.TryGetValue(specificQueryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query handler registered for {specificQueryType.Name} query.");
            }

            QueryHandlerDelegate<TResult> newHandleQueryDelegate = queryHandlerMethod.CreateDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);

            _queryHandlerDelegatesByQueryType.Add(specificQueryType, newHandleQueryDelegate);
        }

        private static IEnumerable<QueryHandlerAttributeMethod> getQueryHandlerMethods(Type queryHandlerType)
        {
            List<QueryHandlerAttributeMethod> queryHandlerMethods = new List<QueryHandlerAttributeMethod>();

            foreach (MethodInfo methodInfo in queryHandlerType.GetRuntimeMethods())
            {
                if (methodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(QueryHandlerAttribute)))
                {
                    // Return methods marked with [QueryHandler].
                    yield return QueryHandlerAttributeMethod.Create(methodInfo);
                }
            }
        }

        #endregion Functions
    }
}
