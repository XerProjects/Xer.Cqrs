using System;
using System.Collections.Generic;

namespace Xer.Cqrs.QueryStack.Resolvers
{
    public class CompositeQueryHandlerResolver : IQueryHandlerResolver
    {
        private readonly IEnumerable<IQueryHandlerResolver> _resolvers;
        private readonly Func<Exception, bool> _exceptionHandler;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resolvers">List of resolvers.</param>
        public CompositeQueryHandlerResolver(IEnumerable<IQueryHandlerResolver> resolvers)
        {
            _resolvers = resolvers;
        }

        /// <summary>
        /// Constructor with exception handler.
        /// </summary>
        /// <param name="resolvers">List of resolvers.</param>
        /// <param name="exceptionHandler">
        /// If exception handler returns true, this resolver will try to resolve a query handler 
        /// from the next resolver in the list. Otherwise, resolve will stop and exception will be re-thrown.
        /// </param>
        public CompositeQueryHandlerResolver(IEnumerable<IQueryHandlerResolver> resolvers, Func<Exception, bool> exceptionHandler)
        {
            _resolvers = resolvers;
            _exceptionHandler = exceptionHandler;
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
                try
                {
                    QueryHandlerDelegate<TResult> commandHandlerDelegate = resolver.ResolveQueryHandler<TQuery, TResult>();
                    if (commandHandlerDelegate != null)
                    {
                        return commandHandlerDelegate;
                    }
                }
                catch(Exception ex)
                {
                    if(_exceptionHandler == null)
                    {
                        // No exception handler. Re-throw exception.
                        throw;
                    }

                    bool handled = _exceptionHandler.Invoke(ex);
                    if(!handled)
                    {
                        // Not handled. Re-throw exception.
                        throw;
                    }
                }
            }

            Type queryType = typeof(TQuery);
            throw new NoQueryHandlerResolvedException($"No query handler is registered to handle query of type: { queryType.Name }", queryType);
        }
    }
}
