using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.UseCases;
using Domain.Commands;
using Domain.Repositories;
using Infrastructure.DomainEventHandlers;
using ReadSide.Products.Repositories;
using ReadSide.Products.Queries;
using SimpleInjector;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Delegator;
using Xer.Delegator.Resolvers;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            using(CancellationTokenSource cts = new CancellationTokenSource())
            {
                App app = Setup(args);
                await app.StartAsync(args, cts.Token);
            }
        }

        static App Setup(string[] args)
        {
            Container container = new Container();
            
            // Register console app use cases.
            container.RegisterCollection(typeof(IUseCase), typeof(IUseCase).Assembly);

            // Product write-side repository.
            container.RegisterSingleton<IProductRepository>(() =>
                new PublishingProductRepository(new InMemoryProductRepository(), container.GetInstance<IEventDelegator>())
            );

            // Product read-side repository.
            container.RegisterSingleton<IProductReadSideRepository, InMemoryProductReadSideRepository>();

            // Register all async command handlers in assembly.
            container.Register(typeof(ICommandAsyncHandler<>), new[] { typeof(RegisterProductCommandHandler).Assembly });

            // Register all async query handlers in assembly.
            container.Register(typeof(IQueryAsyncHandler<,>), new[] { typeof(QueryProductByIdHandler).Assembly });

            // Register all async event handlers in assembly.
            container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(ProductDomainEventsHandler).Assembly);

            // Register container adapters to be used by resolvers.
            container.RegisterSingleton<SimpleInjectorContainerAdapter>(() => new SimpleInjectorContainerAdapter(container));

            // Register command delegator.
            container.RegisterSingleton<ICommandDelegator>(() =>
                new CommandDelegator(new ContainerCommandAsyncHandlerResolver(container.GetInstance<SimpleInjectorContainerAdapter>()))
            );

            // Register event delegator.
            container.RegisterSingleton<IEventDelegator>(() =>
                new EventDelegator(new ContainerEventHandlerResolver(container.GetInstance<SimpleInjectorContainerAdapter>(), 
                                                                     yieldExecutionOfSyncHandlers: true))
            );

            // Register query dispatcher.
            container.RegisterSingleton<IQueryAsyncDispatcher>(() => 
                new QueryDispatcher(new ContainerQueryAsyncHandlerResolver(container.GetInstance<SimpleInjectorContainerAdapter>()))
            );

            container.Verify();
            
            return new App(container);
        }
    }
    
    class SimpleInjectorContainerAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
                                           Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter,
                                           Xer.Cqrs.EventStack.Resolvers.IContainerAdapter
    {
        private readonly Container _container;

        public SimpleInjectorContainerAdapter(Container container)
        {
            _container = container;
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public IEnumerable<T> ResolveMultiple<T>() where T : class
        {
            return _container.GetAllInstances<T>();
        }
    }
}
