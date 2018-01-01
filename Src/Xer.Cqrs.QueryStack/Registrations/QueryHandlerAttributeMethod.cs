using System;
using System.Collections.Generic;
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

        public Type DeclaringType { get; }
        public Type QueryType { get; }
        public Type QueryResultType { get; }
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
        /// <param name="queryResultType">Method's return type. This should match query's result type.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        private QueryHandlerAttributeMethod(MethodInfo methodInfo, Type queryType, Type queryResultType, bool isAsync, bool supportsCancellation)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            DeclaringType = methodInfo.DeclaringType;
            QueryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
            QueryResultType = queryResultType ?? throw new ArgumentNullException(nameof(queryResultType));
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
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

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
        public static QueryHandlerAttributeMethod FromMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

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

            return new QueryHandlerAttributeMethod(methodInfo, queryParameter.ParameterType, queryMethodReturnType, isAsync, supportsCancellation);
        }

        /// <summary>
        /// Create QueryHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have QueryHandlerAttribute custom attributes.</param>
        /// <returns>Instances of QueryHandlerAttributeMethod.</returns>
        public static List<QueryHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(m => FromMethodInfo(m)).ToList();
        }

        /// <summary>
        /// Detect methods marked with [QueryHandler] attribute and translate to QueryHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [QueryHandler] attribute.</param>
        /// <returns>List of all QueryHandlerAttributeMethod detected.</returns>
        public static List<QueryHandlerAttributeMethod> FromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> methods = type.GetRuntimeMethods()
                                                  .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(QueryHandlerAttribute)));

            return methods.Select(m => FromMethodInfo(m)).ToList();
        }

        /// <summary>
        /// Detect methods marked with [QueryHandler] attribute and translate to QueryHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="types">Types to scan for methods marked with the [QueryHandler] attribute.</param>
        /// <returns>List of all QueryHandlerAttributeMethod detected.</returns>
        public static List<QueryHandlerAttributeMethod> FromTypes(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(t => FromType(t)).ToList();
        }

        /// <summary>
        /// Detect methods marked with [QueryHandler] attribute and translate to QueryHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="queryHandlerAssembly">Assembly to scan for methods marked with the [QueryHandler] attribute.</param>
        /// <returns>List of all QueryHandlerAttributeMethod detected.</returns>
        public static List<QueryHandlerAttributeMethod> FromAssembly(Assembly queryHandlerAssembly)
        {
            if (queryHandlerAssembly == null)
            {
                throw new ArgumentNullException(nameof(queryHandlerAssembly));
            }

            IEnumerable<MethodInfo> commandHandlerMethods = queryHandlerAssembly.DefinedTypes.SelectMany(t => 
                                                                t.DeclaredMethods.Where(m => 
                                                                    m.CustomAttributes.Any(a => a.AttributeType == typeof(QueryHandlerAttribute))));
            
            return FromMethodInfos(commandHandlerMethods);
        }

        /// <summary>
        /// Detect methods marked with [QueryHandler] attribute and translate to QueryHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="queryHandlerAssemblies">Assemblies to scan for methods marked with the [QueryHandler] attribute.</param>
        /// <returns>List of all QueryHandlerAttributeMethod detected.</returns>
        public static List<QueryHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> queryHandlerAssemblies)
        {
            if (queryHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(queryHandlerAssemblies));
            }

            return queryHandlerAssemblies.SelectMany(a => FromAssembly(a)).ToList();
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
