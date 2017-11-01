using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks
{
    #region Base Query Handler
    
    public abstract class TestQueryHandlerBase
    {
        private readonly List<object> _handledQueries = new List<object>();

        protected ITestOutputHelper TestOutputHelper { get; }
        public IReadOnlyCollection<object> HandledQueries => _handledQueries.AsReadOnly();

        public TestQueryHandlerBase(ITestOutputHelper output)
        {
            TestOutputHelper = output;
        }

        protected void Handle<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            _handledQueries.Add(query);
            TestOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled a query of type {query.GetType()}.");
        }

        protected void HandleAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            _handledQueries.Add(query);
            TestOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled a query of type {query.GetType()} asynchronously.");
        }
    }

    #endregion Base Query Handler

    #region Query Handlers

    public class TestQueryHandler : TestQueryHandlerBase,
                                    IQueryHandler<QuerySomething, string>,
                                    IQueryHandler<QuerySomethingNonReferenceType, int>,
                                    IQueryHandler<QuerySomethingAsync, string>,
                                    IQueryHandler<QuerySomethingWithException, string>,
                                    IQueryAsyncHandler<QuerySomething, string>,
                                    IQueryAsyncHandler<QuerySomethingNonReferenceType, int>,
                                    IQueryAsyncHandler<QuerySomethingAsync, string>,
                                    IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>,
                                    IQueryAsyncHandler<QuerySomethingWithException, string>
    {
        public TestQueryHandler(ITestOutputHelper output)
            : base(output)
        {
        }

        public string Handle(QuerySomething query)
        {
            base.Handle<QuerySomething, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return query.Data;
        }

        public int Handle(QuerySomethingNonReferenceType query)
        {
            base.Handle<QuerySomethingNonReferenceType, int>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return query.Data;
        }

        public string Handle(QuerySomethingAsync query)
        {
            base.Handle<QuerySomethingAsync, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return query.Data;
        }

        public string Handle(QuerySomethingWithException query)
        {
            base.Handle<QuerySomethingWithException, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            throw new TestQueryHandlerException("This is a triggered post-processing exception.");
        }

        public Task<string> HandleAsync(QuerySomething query, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync<QuerySomething, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return Task.FromResult(query.Data);
        }

        public Task<int> HandleAsync(QuerySomethingNonReferenceType query, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync<QuerySomethingNonReferenceType, int>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return Task.FromResult(query.Data);
        }

        public Task<string> HandleAsync(QuerySomethingAsync query, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync<QuerySomethingAsync, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return Task.FromResult(query.Data);
        }

        public async Task<string> HandleAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync<QuerySomethingAsyncWithDelay, string>(query);

            await Task.Delay(query.DelayInMilliseconds, cancellationToken);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return query.Data;
        }

        public Task<string> HandleAsync(QuerySomethingWithException query, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync<QuerySomethingWithException, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            return Task.FromException<string>(new TestQueryHandlerException("This is a triggered post-processing exception."));
        }
    }

    #endregion Query Handlers

    #region Attributed Query Handlers

    public class TestAttributedQueryHandler : TestQueryHandlerBase
    {
        private static int _instanceCounter = 0;

        public TestAttributedQueryHandler(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _instanceCounter++;
        }

        [QueryHandler]
        public string QuerySomething(QuerySomething query)
        {
            base.Handle<QuerySomething, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            TestOutputHelper.WriteLine($"Instance #{_instanceCounter}.");

            return query.Data;
        }

        [QueryHandler]
        public string QuerySomethingWithException(QuerySomethingWithException query)
        {
            base.Handle<QuerySomethingWithException, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            TestOutputHelper.WriteLine($"Instance #{_instanceCounter}.");

            throw new Exception("This is a triggered post-processing exception.");
        }

        [QueryHandler]
        public int QuerySomething(QuerySomethingNonReferenceType query)
        {
            base.Handle<QuerySomethingNonReferenceType, int>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            TestOutputHelper.WriteLine($"Instance #{_instanceCounter}.");

            return query.Data;
        }

        [QueryHandler]
        public Task<string> QuerySomethingAsync(QuerySomethingAsync query)
        {
            base.HandleAsync<QuerySomethingAsync, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            TestOutputHelper.WriteLine($"Instance #{_instanceCounter}.");

            return Task.FromResult(query.Data);
        }

        [QueryHandler]
        public async Task<string> QuerySomethingAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken)
        {
            base.HandleAsync<QuerySomethingAsyncWithDelay, string>(query);

            TestOutputHelper.WriteLine($"Query result: {query.Data}.");

            TestOutputHelper.WriteLine($"Instance #{_instanceCounter}.");

            await Task.Delay(query.DelayInMilliseconds, cancellationToken);

            return query.Data;
        }
    }

    public class TestAttributedQueryHandlerNoReturnType
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestAttributedQueryHandlerNoReturnType(ITestOutputHelper testOutputHelper)
        {
            _outputHelper = testOutputHelper;
        }

        [QueryHandler]
        public void ThisShouldNotBeAllowed(QuerySomething query)
        {
            _outputHelper.WriteLine(query.Data);
        }
    }

    #endregion Attributed Query Handlers

    public class TestQueryHandlerException : Exception
    {
        public TestQueryHandlerException() { }
        public TestQueryHandlerException(string message) : base(message) { }
    }
}
