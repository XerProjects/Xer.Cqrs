using System;
using System.Collections.Generic;

namespace Xer.Cqrs.QueryStack.Resolvers
{
    public class CompositeQueryHandlerResolver : IQueryHandlerResolver
    {
        private readonly IEnumerable<IQueryHandlerResolver> _resolvers;

        public CompositeQueryHandlerResolver(IEnumerable<IQueryHandlerResolver> resolvers)
        {
            _resolvers = resolvers;
        }

        /// <summary>
        /// Get the registered query handler delegate to handle the query of the specified type.
        /// </summary>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
        {
            foreach (IQueryHandlerResolver resolver in _resolvers)
            {
                QueryHandlerDelegate<TResult> handlerDelegate = resolver.ResolveQueryHandler<TQuery, TResult>();
                if (handlerDelegate != null)
                {
                    return handlerDelegate;
                }
            }

            throw new QueryNotHandledException($"No query handler is registered to handle query of type: { typeof(TQuery).Name }");
        }
    }
}
