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

        public QueryHandlerDelegate<TResult> ResolveQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult>
        {
            IQueryAsyncHandler<TQuery, TResult> queryAsyncHandler = _containerAdapter.Resolve<IQueryAsyncHandler<TQuery, TResult>>();

            if(queryAsyncHandler != null)
            {
                return new QueryHandlerDelegate<TResult>((q, ct) =>
                {
                    return queryAsyncHandler.HandleAsync((TQuery)q, ct);
                });
            }

            IQueryHandler<TQuery, TResult> queryHandler = _containerAdapter.Resolve<IQueryHandler<TQuery, TResult>>();

            if (queryHandler != null)
            {
                return new QueryHandlerDelegate<TResult>((q, ct) =>
                {
                    TResult result = queryHandler.Handle((TQuery)q);
                    return Task.FromResult(result);
                });
            }

            throw new QueryNotHandledException($"No query handler is registered in the container to handles queries of type: {typeof(TQuery).Name}.");
        }
    }

    public interface IContainerAdapter
    {
        /// <summary>
        /// Resolve instance from a container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve from container.</typeparam>
        /// <returns>Instance of requested object.</returns>
        T Resolve<T>();
    }
}
