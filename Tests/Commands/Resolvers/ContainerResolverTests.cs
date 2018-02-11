using SimpleInjector;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.Tests.Entities;
using Xer.Delegator;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
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
            public void Should_Resolve_Correct_Sync_Command_Handler()
            {
                var container = new Container();
                container.Register<ICommandHandler<TestCommand>>(() => new TestCommandHandler(_outputHelper), Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerCommandHandlerResolver(containerAdapter);

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));
                
                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                // Get instance from container to see if it was invoked.
                var registeredCommandHandler = (TestCommandHandler)container.GetInstance<ICommandHandler<TestCommand>>();

                Assert.Equal(1, registeredCommandHandler.HandledCommands.Count);
                Assert.Contains(registeredCommandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public void Should_Resolve_Correct_Async_Command_Handler()
            {
                var container = new Container();
                container.Register<ICommandAsyncHandler<TestCommand>>(() => new TestCommandHandler(_outputHelper), Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerCommandAsyncHandlerResolver(containerAdapter);

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));
                
                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                // Get instance from container to see if it was invoked.
                var registeredCommandHandler = (TestCommandHandler)container.GetInstance<ICommandAsyncHandler<TestCommand>>();

                Assert.Equal(1, registeredCommandHandler.HandledCommands.Count);
                Assert.True(registeredCommandHandler.HasHandledCommand<TestCommand>());
            }
        }

        #endregion Register Method
    }
}
