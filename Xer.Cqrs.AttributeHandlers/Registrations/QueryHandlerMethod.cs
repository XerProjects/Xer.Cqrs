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
    internal class QueryHandlerMethod
    {
        private static readonly TypeInfo TaskTypeInfo = typeof(Task).GetTypeInfo();

        public Type QueryType { get; }
        public Type QueryReturnType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        private QueryHandlerMethod(Type queryType, Type queryReturnType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
        {
            QueryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
            QueryReturnType = queryReturnType ?? throw new ArgumentNullException(nameof(queryReturnType));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #region Methods
        
        public QueryAsyncHandlerDelegate<TResult> CreateDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TQuery : IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate;

            if (IsAsync)
            {

                if (SupportsCancellation)
                {
                    newHandleQueryDelegate = createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
                }
                else
                {
                    newHandleQueryDelegate = createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
                }
            }
            else
            {
                newHandleQueryDelegate = createWrappedSyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
            }

            return newHandleQueryDelegate;
        }

        public static QueryHandlerMethod Create(MethodInfo methodInfo)
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

            if (TaskTypeInfo.IsAssignableFrom(queryMethodReturnType.GetTypeInfo()))
            {
                isAsync = true;
                supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

                // Get actual return type from Task's generic argument.
                queryMethodReturnType = queryMethodReturnType.GenericTypeArguments.FirstOrDefault();

                if (queryMethodReturnType == null)
                {
                    // Task does not have a generic argument. So, no return value.
                    throw new InvalidOperationException($"Method marked with [QueryHandler] should have a return value. Task should have a result: {methodInfo.Name}");
                }
            }

            TypeInfo genericQueryTypeInfo = typeof(IQuery<>).MakeGenericType(queryMethodReturnType).GetTypeInfo();

            ParameterInfo queryParameter = methodParameters.FirstOrDefault(p => genericQueryTypeInfo.IsAssignableFrom(p.ParameterType.GetTypeInfo()));

            if (queryParameter == null)
            {
                // Method return type does not match return type of query parameter. Skip.
                throw new InvalidOperationException($"Methods marked with [QueryHandler] should accept a query parameter and return type should match expected result of the query it's accepting: {methodInfo.Name}");
            }

            return new QueryHandlerMethod(queryParameter.ParameterType, queryMethodReturnType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        private QueryAsyncHandlerDelegate<TResult> createWrappedSyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TQuery : IQuery<TResult>
        {
            AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult>)MethodInfo.CreateDelegate(typeof(AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                TResult result = func.Invoke(instance, (TQuery)q);

                return Task.FromResult(result);
            };

            return newHandleQueryDelegate;
        }

        private QueryAsyncHandlerDelegate<TResult> createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TQuery : IQuery<TResult>
        {
            AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult>)MethodInfo.CreateDelegate(typeof(AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                return func.Invoke(instance, (TQuery)q, ct);
            };

            return newHandleQueryDelegate;
        }

        private QueryAsyncHandlerDelegate<TResult> createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TQuery : IQuery<TResult>
        {
            AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult> func = (AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult>)MethodInfo.CreateDelegate(typeof(AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult>));

            QueryAsyncHandlerDelegate<TResult> newHandleQueryDelegate = (q, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a query handler instance for {q.GetType().Name}");
                }

                return func.Invoke(instance, (TQuery)q);
            };

            return newHandleQueryDelegate;
        }

        #endregion Functions
    }
}
