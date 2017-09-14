using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers
{
    public class QueryHandlerRegistration
    {
        private readonly Dictionary<Type, HandleQueryDelegate> handleQueryDelegatesByQueryType = new Dictionary<Type, HandleQueryDelegate>();
        
        public void Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult> where TResult : class
        {
            Type queryType = typeof(TQuery);

            HandleQueryDelegate handleQueryDelegate = (q) =>
            {
                var queryResult = queryHandler.Handle((TQuery)q);
                return Task.FromResult<object>(queryResult);
            };

            handleQueryDelegatesByQueryType.Add(queryType, handleQueryDelegate);
        }

        public void Register<TQuery, TResult>(IQueryAsyncHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult> where TResult : class
        {
            Type queryType = typeof(TQuery);

            HandleQueryDelegate handleQueryDelegate = async (q) =>
            {
                return await queryHandler.HandleAsync((TQuery)q);
            };

            handleQueryDelegatesByQueryType.Add(queryType, handleQueryDelegate);
        }
        
        internal HandleQueryDelegate GetRegisteredQueryHandler(Type queryType)
        {
            HandleQueryDelegate handleQueryDelegate;

            if (!handleQueryDelegatesByQueryType.TryGetValue(queryType, out handleQueryDelegate))
            {
                throw new NotSupportedException($"No query handler is registered to handle queries of type: { queryType.Name }");
            }

            return handleQueryDelegate;
        }
    }
}
