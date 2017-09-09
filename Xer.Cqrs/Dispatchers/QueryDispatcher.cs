using System;
using System.Collections.Generic;

namespace Xer.Cqrs.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly Dictionary<Type, Func<IQuery<object>, object>> _queryHandlerActionsByQueryType = new Dictionary<Type, Func<IQuery<object>, object>>();

        public virtual TResult Dispatch<TResult>(IQuery<TResult> query) where TResult : class
        {
            Type queryType = query.GetType();

            Func<IQuery<object>, object> handleQueryAction;

            if(!_queryHandlerActionsByQueryType.TryGetValue(queryType, out handleQueryAction))
            {
                throw new NotSupportedException($"No query handler is registered to handle queries of type: { queryType.Name }");
            }

            return (TResult)handleQueryAction.Invoke(query);
        }

        public void RegisterHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult> where TResult : class
        {
            Type handlerType = queryHandler.GetType();
            Type queryType = typeof(TQuery);

            Func<IQuery<object>, object> actionHandler = (q) => queryHandler.Handle((TQuery)q);

            _queryHandlerActionsByQueryType.Add(queryType, actionHandler);
        }
    }
}
