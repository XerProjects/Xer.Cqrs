using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.AttributeHandlers.Registrations
{
    public class QueryHandlerAttributeRegistration : IAttributedHandlerRegistration, IQueryHandlerProvider
    {
        #region Declarations

        private static readonly Type QueryHandlerAttributeType = typeof(QueryHandlerAttribute);

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
            var methods = queryHandlerType.GetRuntimeMethods()
                           .Where(m => m.CustomAttributes.Any(a => a.AttributeType == QueryHandlerAttributeType));

            List<QueryHandlerMethod> queryHandlerMethods = new List<QueryHandlerMethod>();

            foreach(MethodInfo methodInfo in methods)
            {
                Type queryMethodReturnType = methodInfo.ReturnType;
                if (queryMethodReturnType == typeof(void))
                {
                    // Query handler method needs a return type.
                    throw new InvalidOperationException($"Method marked with [QueryHandler] should have a return value: {methodInfo.Name}");
                }

                ParameterInfo[] methodParameters = methodInfo.GetParameters();
                bool isAsync = false;
                bool supportsCancellation = false;

                if (typeof(Task).GetTypeInfo().IsAssignableFrom(queryMethodReturnType.GetTypeInfo()))
                {
                    isAsync = true;
                    supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

                    try
                    {
                        // Get actual return type from Task's generic argument.
                        queryMethodReturnType = queryMethodReturnType.GenericTypeArguments.SingleOrDefault();
                    }
                    catch(Exception)
                    {
                        // Task has multiple generic arguments.
                        throw new InvalidOperationException($"Method marked with [QueryHandler] should have a return value. If return type is a Task, it should only have a single result: {methodInfo.Name}");
                    }

                    if (queryMethodReturnType == null)
                    {
                        // Task does not have a generic argument. So, no return value.
                        throw new InvalidOperationException($"Method marked with [QueryHandler] should have a return value. Task should have a result: {methodInfo.Name}");
                    }
                }

                Type genericQueryType = typeof(IQuery<>).MakeGenericType(queryMethodReturnType);

                ParameterInfo queryParameter = methodParameters.FirstOrDefault(p => genericQueryType.GetTypeInfo().IsAssignableFrom(p.ParameterType.GetTypeInfo()));

                if (queryParameter == null)
                {
                    // Method return type does not match return type of query parameter. Skip.
                    throw new InvalidOperationException($"Methods marked with [QueryHandler] should accept a query parameter and return type should match expected result of the query it's accepting: {methodInfo.Name}");
                }

                queryHandlerMethods.Add(new QueryHandlerMethod(queryParameter.ParameterType, queryMethodReturnType, methodInfo, isAsync, supportsCancellation));
            }

            return queryHandlerMethods;
        }

        private void registerQueryHandlerMethod<TAttributed, TQuery, TResult>(Func<TAttributed> attributedHandlerFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);

            QueryAsyncHandlerDelegate<TResult> handleQueryDelegate;
            if (_queryHandlerDelegatesByQueryType.TryGetValue(specificQueryType, out handleQueryDelegate))
            {
                throw new InvalidOperationException($"Duplicate query async handler registered for {specificQueryType.Name} query.");
            }

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate;

            if (queryHandlerMethod.IsAsync)
            {
                newHandleQueryDelegate = createAsyncDelegate<TAttributed, TQuery, TResult>(attributedHandlerFactory, queryHandlerMethod);
            }
            else
            {
                newHandleQueryDelegate = createWrappedSyncDelegate<TAttributed, TQuery, TResult>(attributedHandlerFactory, queryHandlerMethod);
            }

            _queryHandlerDelegatesByQueryType.Add(specificQueryType, newHandleQueryDelegate);
        }

        private QueryAsyncHandlerDelegate<TResult> createWrappedSyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedHandlerFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult>)queryHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedHandlerFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                TResult result = func.Invoke(instance, (TQuery)q);

                return Task.FromResult(result);
            };

            return newHandleQueryDelegate;
        }

        private static QueryAsyncHandlerDelegate<TResult> createAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedHandlerFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);
            
            if (queryHandlerMethod.SupportsCancellation)
            {
                return createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedHandlerFactory, queryHandlerMethod);
            }
            else
            {
                return createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedHandlerFactory, queryHandlerMethod);
            }
        }

        private static QueryAsyncHandlerDelegate<TResult> createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedHandlerFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult>)queryHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedHandlerFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                return func.Invoke(instance, (TQuery)q, ct);
            };

            return newHandleQueryDelegate;
        }

        private static QueryAsyncHandlerDelegate<TResult> createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedHandlerFactory, QueryHandlerMethod queryHandlerMethod) where TQuery : IQuery<TResult>
        {
            AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult>)queryHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedHandlerFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                return func.Invoke(instance, (TQuery)q);
            };

            return newHandleQueryDelegate;
        }

        #endregion Functions

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
        
        #region Inner QueryHandlerMethod Class

        private class QueryHandlerMethod
        {
            public Type QueryType { get; }
            public Type QueryReturnType { get; }
            public MethodInfo MethodInfo { get; }
            public bool IsAsync { get; private set; }
            public bool SupportsCancellation { get; }

            public QueryHandlerMethod(Type queryType, Type queryReturnType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
            {
                QueryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
                QueryReturnType = queryReturnType ?? throw new ArgumentNullException(nameof(queryReturnType));
                MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
                IsAsync = isAsync;
                SupportsCancellation = supportsCancellation;
            }
        }

        #endregion Inner QueryHandlerMethod Class
    }
}
