using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Console.EventHandlers;
using Domain.Commands;
using Domain.Queries;
using Domain.Repositories;
using SimpleInjector;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Resolvers;

namespace Console
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            using(CancellationTokenSource cts = new CancellationTokenSource())
            {
                App app = Setup(args);
                await app.StartAsync(args, cts.Token);

                System.Console.WriteLine("Exiting console application...");
            }
        }

        private static App Setup(string[] args)
        {
            Container container = new Container();

            container.RegisterSingleton<IProductRepository, InMemoryProductRepository>();

            // Register all async command handlers in assembly.
            container.Register(typeof(ICommandAsyncHandler<>), new[] { typeof(RegisterProductCommandHandler).Assembly });

            // Register all async query handlers in assembly.
            container.Register(typeof(IQueryAsyncHandler<,>), new[] { typeof(QueryProductByIdHandler).Assembly });

            // Register all sync and async event handlers in assembly.
            container.RegisterCollection(typeof(IEventHandler<>), typeof(ProductRegisteredEventHandler).Assembly);
            container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(ProductRegisteredEventHandler).Assembly);

            var containerAdapter = new SimpleInjectorContainerAdapter(container);
            var commandDispatcher = new CommandDispatcher(new ContainerCommandAsyncHandlerResolver(containerAdapter));
            var queryDispatcher = new QueryDispatcher(new ContainerQueryAsyncHandlerResolver(containerAdapter));
            var eventPublisher = new EventPublisher(new ContainerEventHandlerResolver(containerAdapter));
            
            return new App(commandDispatcher, queryDispatcher, eventPublisher);
        }
    }
    
    public class SimpleInjectorContainerAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
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
