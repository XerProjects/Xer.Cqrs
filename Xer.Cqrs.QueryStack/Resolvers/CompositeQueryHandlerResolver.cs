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
        /// Get the registered query handler delegate to handle the query of the specified type from multiple sources.
        /// </summary>
        /// <typeparam name="TQuery">Type of query to be handled.</typeparam>
        /// <typeparam name="TResult">Type of query's result.</typeparam>
        /// <returns>Instance of <see cref="QueryHandlerDelegate{TResult}"/> which executes the query handler processing.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
        {
            foreach (IQueryHandlerResolver resolver in _resolvers)
            {
                QueryHandlerDelegate<TResult> commandHandlerDelegate = resolver.ResolveQueryHandler<TQuery, TResult>();
                if (commandHandlerDelegate != null)
                {
                    return commandHandlerDelegate;
                }
            }

            throw new NoQueryHandlerResolvedException($"No query handler is registered to handle query of type: { typeof(TQuery).Name }");
        }
    }
}
