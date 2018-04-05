using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xunit.Abstractions;

namespace Xer.Cqrs.QueryStack.Tests.Entities
{
    public class LoggingQueryDispatcher : IQueryAsyncDispatcher, IQueryDispatcher
    {
        private readonly QueryDispatcher _queryDispatcher;
        private readonly ITestOutputHelper _outputHelper;

        public LoggingQueryDispatcher(QueryDispatcher queryDispatcher, ITestOutputHelper outputHelper)
        {
            _queryDispatcher = queryDispatcher;
            _outputHelper = outputHelper;
        }

        public TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : class, IQuery<TResult>
        {
            try
            {
                return _queryDispatcher.Dispatch<TQuery, TResult>(query);
            }
            catch(Exception ex)
            {
                _outputHelper.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default(CancellationToken)) where TQuery : class, IQuery<TResult>
        {
            try
            {
                return await _queryDispatcher.DispatchAsync<TQuery, TResult>(query);
            }
            catch(Exception ex)
            {
                _outputHelper.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}