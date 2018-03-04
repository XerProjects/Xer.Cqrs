using System;
using System.Threading.Tasks;
using FluentAssertions;
using SimpleInjector;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.CommandStack.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Exceptions;
using Xer.Delegator.Resolvers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.CommandStack.Tests.Resolvers
{
    public class ContainerResolverTests
    {
        #region Register Method

        public class ResolveMessageHandlerMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public ResolveMessageHandlerMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void ShouldResolveSyncCommandHandlerFromContainer()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);

                IMessageHandlerResolver resolver = CreateContainerCommandHandlerResolver(container => 
                {
                    container.RegisterSingleton<ICommandHandler<TestCommand>>(commandHandler);
                });

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));
                
                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }

            [Fact]
            public void ShouldResolveAsyncCommandHandlerFromContainer()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);

                IMessageHandlerResolver resolver = CreateContainerCommandAsyncHandlerResolver(container => 
                {
                    container.RegisterSingleton<ICommandAsyncHandler<TestCommand>>(commandHandler);
                });

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));
                
                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }

            [Fact]
            public void ShouldResolveCommandHandlerFromCompositeResolver()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);
                var container = new Container();
                container.RegisterSingleton<ICommandHandler<TestCommand>>(commandHandler);
                container.RegisterSingleton<ICommandAsyncHandler<CancellableTestCommand>>(commandHandler);

                // Exception handler will log and ignore exception.
                Func<Exception, bool> exceptionHandler = (ex) => 
                {
                    if (ex != null) 
                    {
                        _outputHelper.WriteLine($"Ignoring encountered exception while trying to resolve command handler: {ex.Message}");
                        
                        // Notify as handled if no command handler is resolved from other resolvers.
                        return true;
                    }

                    return false;
                };

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var containerAsyncHandlerResolver = new ContainerCommandAsyncHandlerResolver(containerAdapter, exceptionHandler);
                var containerHandlerResolver = new ContainerCommandHandlerResolver(containerAdapter, exceptionHandler);

                CompositeMessageHandlerResolver compositeResolver = CompositeMessageHandlerResolver.Compose(
                    containerAsyncHandlerResolver,
                    containerHandlerResolver);

                MessageHandlerDelegate testCommandHandlerDelegate = compositeResolver.ResolveMessageHandler(typeof(TestCommand));
                MessageHandlerDelegate cancellableTestCommandHandlerDelegate = compositeResolver.ResolveMessageHandler(typeof(CancellableTestCommand));

                testCommandHandlerDelegate?.Invoke(new TestCommand());
                cancellableTestCommandHandlerDelegate?.Invoke(new CancellableTestCommand());

                commandHandler.HandledCommands.Should().HaveCount(2);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
                commandHandler.HasHandledCommand<CancellableTestCommand>().Should().BeTrue();
            }

            [Fact]
            public void ShouldThrowWhenNoRegisteredCommandHandlerIsFoundInContainer()
            { 
                var container = new Container();
                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                
                var delegator = new LoggingCommandDelegator(
                    new CommandDelegator(new ContainerCommandHandlerResolver(containerAdapter)), // Sync handler resolver
                    _outputHelper
                ); 

                Func<Task> sendAsyncAction = async () => await delegator.SendAsync(new TestCommand());

                sendAsyncAction.Should().Throw<NoMessageHandlerResolvedException>();
            }

            public static IMessageHandlerResolver CreateContainerCommandAsyncHandlerResolver(Action<Container> containerSetup)
            {
                var container = new Container();
                containerSetup?.Invoke(container);
                return new ContainerCommandAsyncHandlerResolver(new SimpleInjectorContainerAdapter(container));
            }

            public static IMessageHandlerResolver CreateContainerCommandHandlerResolver(Action<Container> containerSetup)
            {
                var container = new Container();
                containerSetup?.Invoke(container);
                return new ContainerCommandHandlerResolver(new SimpleInjectorContainerAdapter(container));
            }

            public static CommandDelegator CreateCommandDelegator(IMessageHandlerResolver resolver)
            {             
                return new CommandDelegator(resolver);
            }
        }

        #endregion Register Method
    }
}
