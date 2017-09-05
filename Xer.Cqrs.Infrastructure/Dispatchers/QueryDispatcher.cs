using Xer.Cqrs.Infrastructure.Queries;
using Xer.Cqrs.Queries;
using System;
using System.Collections.Generic;

namespace Xer.Cqrs.Infrastructure.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly Dictionary<Type, Func<IQuery<object>, object>> _handlersByQueryType = new Dictionary<Type, Func<IQuery<object>, object>>();

        public TResult Dispatch<TResult>(IQuery<TResult> query)
        {
            Type queryType = query.GetType();

            Func<IQuery<object>, object> handler;

            if(!_handlersByQueryType.TryGetValue(queryType, out handler))
            {
                throw new NotSupportedException($"No handler is found for query if type: { queryType.Name }");
            }

            return (TResult)handler.Invoke((IQuery<object>)query);
        }

        public void RegisterHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult>
        {
            Type handlerType = queryHandler.GetType();
            Type queryType = typeof(TQuery);

            var actionHandler = new Func<IQuery<object>, object>((q) => queryHandler.Handle((TQuery)q));

            _handlersByQueryType.Add(queryType, actionHandler);
        }
    }
}
