using System;

namespace Xer.Cqrs.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly QueryHandlerRegistration _registration;

        public QueryDispatcher(QueryHandlerRegistration registration)
        {
            _registration = registration;
        }

        public virtual TResult Dispatch<TResult>(IQuery<TResult> query) where TResult : class
        {
            Type queryType = query.GetType();

            HandleQueryDelegate handleQueryDelegate = _registration.GetRegisteredQueryHandler(queryType);

            // Wait synchronously.
            return (TResult)handleQueryDelegate.Invoke(query).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
