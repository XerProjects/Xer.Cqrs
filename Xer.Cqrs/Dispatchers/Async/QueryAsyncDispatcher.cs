using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers.Async
{
    public class QueryAsyncDispatcher : IQueryAsyncDispatcher
    {
        private readonly QueryHandlerRegistration _registration;

        public QueryAsyncDispatcher(QueryHandlerRegistration registration)
        {
            _registration = registration;
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
        {
            Type queryType = query.GetType();

            HandleQueryDelegate handleQueryDelegate = _registration.GetRegisteredQueryHandler(queryType);

            return (TResult) await handleQueryDelegate.Invoke(query).ConfigureAwait(false);
        }
    }
}
