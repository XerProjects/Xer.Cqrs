using System;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Resolvers
{
    public class ContainerQueryHandlerResolver : IQueryHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerQueryHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        /// <summary>
        /// <para>Resolves an instance of IQueryAsyncHandler<TQuery, TResult> of IQueryHandler<TQuery, TResult> which handles the given query type</para>
        /// <para>from the container and convert to a query handler delegate which invokes the query handler.</para>
        /// </summary>
        /// <typeparamref name="TQuery">Type of query which is handled by the query handler to resolve.</typeparamref>
        /// <typeparamref name="TResult">Type of query result.</typeparamref>
        /// <returns>A query handler delegate which invokes the query handler and returns the result.</returns>
        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : class, IQuery<TResult>
        {
            try
            {
                IQueryAsyncHandler<TQuery, TResult> queryAsyncHandler = _containerAdapter.Resolve<IQueryAsyncHandler<TQuery, TResult>>();

                if (queryAsyncHandler != null)
                {
                    return QueryHandlerDelegateBuilder.FromQueryHandler(queryAsyncHandler);
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }

            try
            {
                // Try resolving sync query handler next.
                IQueryHandler<TQuery, TResult> queryHandler = _containerAdapter.Resolve<IQueryHandler<TQuery, TResult>>();

                if (queryHandler != null)
                {
                    return QueryHandlerDelegateBuilder.FromQueryHandler(queryHandler);
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }

            // No handlers are resolved. Throw exception.
            throw new QueryNotHandledException($"Unable to resolve a query handler from the container to handle query of type: {typeof(TQuery).Name}.");
        }
    }

    public interface IContainerAdapter
    {
        /// <summary>
        /// Resolve instance from a container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve from container.</typeparam>
        /// <returns>Instance of requested object.</returns>
        T Resolve<T>() where T : class;
    }
}
