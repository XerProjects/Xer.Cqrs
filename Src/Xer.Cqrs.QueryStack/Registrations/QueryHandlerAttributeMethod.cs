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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodInfo">Method info.</param>
        /// <param name="queryType">Type of query that is accepted by this method.</param>
        /// <param name="queryReturnType">Method's return type. This should match query's result type.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        private QueryHandlerAttributeMethod(MethodInfo methodInfo, Type queryType, Type queryReturnType,bool isAsync, bool supportsCancellation)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            QueryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
            QueryReturnType = queryReturnType ?? throw new ArgumentNullException(nameof(queryReturnType));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Create a QueryHandlerDelegate based on the internal method info.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [QueryHandler].</typeparam>
        /// <typeparam name="TQuery">Type of query that is handled by the QueryHandlerDelegate.</typeparam>
        /// <typeparam name="TResult">Query's result type.</typeparam>
        /// <param name="attributedObjectFactory">Factory which returns an instance of the object with methods that are marked with QueryHandlerAttribute.</param>
        /// <returns>Instance of QueryHandlerDelegate.</returns>
        public QueryHandlerDelegate<TResult> CreateDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
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

        /// <summary>
        /// Create QueryHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfo">Method info that has QueryHandlerAttribute custom attribute.</param>
        /// <returns>Instance of QueryHandlerAttributeMethod.</returns>
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

            return new QueryHandlerAttributeMethod(methodInfo, queryParameter.ParameterType, queryMethodReturnType,isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [QueryHandler].</typeparam>
        /// <typeparam name="TQuery">Type of query that is handled by the QueryHandlerDelegate.</typeparam>
        /// <typeparam name="TResult">Query's result type.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of QueryHandlerDelegate.</returns>
        private QueryHandlerDelegate<TResult> createWrappedSyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, TResult> func = (Func<TAttributed, TQuery, TResult>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, TResult>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, func);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [QueryHandler].</typeparam>
        /// <typeparam name="TQuery">Type of query that is handled by the QueryHandlerDelegate.</typeparam>
        /// <typeparam name="TResult">Query's result type.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of QueryHandlerDelegate.</returns>
        private QueryHandlerDelegate<TResult> createCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, CancellationToken, Task<TResult>> asyncCancellableFunc = (Func<TAttributed, TQuery, CancellationToken, Task<TResult>>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, CancellationToken, Task<TResult>>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncCancellableFunc);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [QueryHandler].</typeparam>
        /// <typeparam name="TQuery">Type of query that is handled by the QueryHandlerDelegate.</typeparam>
        /// <typeparam name="TResult">Query's result type.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of QueryHandlerDelegate.</returns>
        private QueryHandlerDelegate<TResult> createNonCancellableAsyncDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            Func<TAttributed, TQuery, Task<TResult>> asyncFunc = (Func<TAttributed, TQuery, Task<TResult>>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TQuery, Task<TResult>>));

            return QueryHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncFunc);
        }

        #endregion Functions
    }
}
