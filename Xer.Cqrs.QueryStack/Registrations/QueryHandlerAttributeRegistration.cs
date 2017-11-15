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
        /// Register methods marked with the [QueryHandler] attribute as query handlers.
        /// <para>Supported signatures for methods marked with [QueryHandler] are: (Methods can be named differently)</para>
        /// <para>TResult HandleQuery(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query);</para>
        /// <para>Task&lt;TResult&gt; HandleQueryAsync(TQuery query, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [QueryHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        public void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class
        {
            if (attributedHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedHandlerFactory));
            }

            Type attributedHandlerType = typeof(TAttributed);

            // Get all public methods marked with CommandHandler attribute.
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
        /// Get the registered query handler delegate which handles the query of the specified type.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <returns>Instance of <see cref="QueryHandlerDelegate{TResult}"/> which executes the query handler processing.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
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

        private void registerQueryHandlerMethod<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, QueryHandlerAttributeMethod queryHandlerMethod) where TAttributed : class 
                                                                                   where TQuery : class, IQuery<TResult>
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
            IEnumerable<MethodInfo> methods = queryHandlerType.GetRuntimeMethods().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(QueryHandlerAttribute)));

            List<QueryHandlerAttributeMethod> queryHandlerMethods = new List<QueryHandlerAttributeMethod>(methods.Count());

            foreach (MethodInfo methodInfo in methods)
            {
                // Return methods marked with [QueryHandler].
                queryHandlerMethods.Add(QueryHandlerAttributeMethod.Create(methodInfo));
            }

            return queryHandlerMethods;
        }

        #endregion Functions
    }
}
