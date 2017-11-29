using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Resolvers;

namespace Console.QueryHandlingDemo
{
    public class Demo
    {
        private QueryDispatcher _dispatcherWithContainerRegistration;
        private QueryDispatcher _dispatcherWithAttributeRegistration;
        private QueryDispatcher _dispatcherWithBasicRegistration;

        public Demo()
        {
            _dispatcherWithContainerRegistration = SetupDispatcherWithContainerRegistration();
            _dispatcherWithAttributeRegistration = SetupDispatcherWithAttributeRegistration();
            _dispatcherWithBasicRegistration = SetupDispatcherWithBasicRegistration();
        }

        private QueryDispatcher SetupDispatcherWithContainerRegistration()
        {
            // Register any command handlers to a container of your choice.
            var container = new Container();
            container.Register<IQueryHandler<SampleQuery, SampleResult>, SampleQueryHandler>();
            
            // Enable QueryDispatcher to resolve query handlers from the container 
            // by creating an implementation of IContainerAdapter and passing it to the 
            // ContainerQueryHandlerResolver which can then used by the dispatcher.
            return new QueryDispatcher(new ContainerQueryHandlerResolver(
                                            new SimpleInjectorContainerAdapter(container)));
        }

        private QueryDispatcher SetupDispatcherWithAttributeRegistration()
        {
            // Register any methods marked with [QueryHandler] 
            // which will be invoked when resolved by the QueryDispatcher.
            var attributeRegistration = new QueryHandlerAttributeRegistration();
            attributeRegistration.Register(() => new SampleQueryHandlerAttributeHandler());

            return new QueryDispatcher(attributeRegistration);
        }

        private QueryDispatcher SetupDispatcherWithBasicRegistration()
        {
            // Register any implementations of IQueryAsyncHandler/IQueryHandler
            // which will be invoked when resolved by the QueryDispatcher.
            var registration = new QueryHandlerRegistration();
            registration.Register<SampleQuery, SampleResult>(() => new SampleQueryAsyncHandler());

            return new QueryDispatcher(registration);
        }

        public async Task ExecuteDemoAsync()
        {
            // Dispatch queries to the registered query handler.
            SampleResult result1 = await _dispatcherWithContainerRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
            System.Console.WriteLine($"Received result from {result1.QueryHandlerName}.");
            
            SampleResult result2 = await _dispatcherWithAttributeRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
            System.Console.WriteLine($"Received result from {result2.QueryHandlerName}.");
            
            SampleResult result3 = await _dispatcherWithBasicRegistration.DispatchAsync<SampleQuery, SampleResult>(new SampleQuery());
            System.Console.WriteLine($"Received result from {result3.QueryHandlerName}.");
        }
    }

    #region Queries

    class SampleQuery : IQuery<SampleResult>
    {

    }

    class SampleResult
    {
        public string QueryHandlerName { get; }

        public SampleResult(string queryHandlerName)
        {
            QueryHandlerName = queryHandlerName;
        }
    }

    class SampleQueryHandler : IQueryHandler<SampleQuery, SampleResult>
    {
        public SampleResult Handle(SampleQuery query)
        {
            System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
            return new SampleResult(GetType().Name);
        }
    }

    class SampleQueryAsyncHandler : IQueryAsyncHandler<SampleQuery, SampleResult>
    {
        public Task<SampleResult> HandleAsync(SampleQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
            return Task.FromResult(new SampleResult(GetType().Name));
        }
    }

    class SampleQueryHandlerAttributeHandler
    {
        [QueryHandler]
        public Task<SampleResult> HandleSampleQueryAsync(SampleQuery query, CancellationToken cancellationToken)
        {
            System.Console.WriteLine($"{GetType().Name} handled {query.GetType().Name} query.");
            return Task.FromResult(new SampleResult(GetType().Name));
        }
    }

    #endregion Queries
}