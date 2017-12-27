using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Registrations
{
    internal class QueryHandlerAttributeMethod
    {
        #region Declarations

        private static readonly TypeInfo TaskTypeInfo = typeof(Task).GetTypeInfo();

        #endregion Declarations

        #region Properties

        public Type QueryType { get; }
        public Type QueryReturnType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        #endregion Properties

        #region Constructors

        private QueryHandlerAttributeMethod(Type queryType, Type queryReturnType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
        {
            QueryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
            QueryReturnType = queryReturnType ?? throw new ArgumentNullException(nameof(queryReturnType));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        public QueryHandlerDelegate<TResult> CreateDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                   where TQuery : class, IQuery<TResult>
        {
            Type specificQueryType = typeof(TQuery);

            if (IsAsync)
            {
                if (SupportsCancellation)
                {
                    return createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
                }
                else
                {
                    return createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
                }
            }
            else
            {
                return createWrappedSyncDelegate<TAttributed, TQuery, TResult>(attributedObjectFactory);
            }
        }

        public static QueryHandlerAttributeMethod Create(MethodInfo methodInfo)
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

            if (!isAsync && supportsCancellation)
            {
                throw new InvalidOperationException("Cancellation token support is only available for async methods (Methods returning a Task).");
            }

            TypeInfo genericQueryTypeInfo = typeof(IQuery<>).MakeGenericType(queryMethodReturnType).GetTypeInfo();

            ParameterInfo queryParameter = methodParameters.FirstOrDefault(p => genericQueryTypeInfo.IsAssignableFrom(p.ParameterType.GetTypeInfo()));

            if (queryParameter == null)
            {
                // Method return type does not match return type of query parameter. Skip.
                throw new InvalidOperationException($"Methods marked with [QueryHandler] should accept a query parameter and return type should match expected result of the query it's accepting: {methodInfo.Name}");
            }

            return new QueryHandlerAttributeMethod(queryParameter.ParameterType, queryMethodReturnType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        private QueryHandlerDelegate<TResult> createWrappedSyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                      where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, TResult> func = (Func<TAttributed, TQuery, TResult>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, TResult>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, func);
        }

        private QueryHandlerDelegate<TResult> createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                      where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, CancellationToken, Task<TResult>> asyncCancellableFunc = (Func<TAttributed, TQuery, CancellationToken, Task<TResult>>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, CancellationToken, Task<TResult>>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncCancellableFunc);
        }

        private QueryHandlerDelegate<TResult> createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                      where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, Task<TResult>> asyncFunc = (Func<TAttributed, TQuery, Task<TResult>>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, Task<TResult>>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncFunc);
        }

        #endregion Functions
    }
}
