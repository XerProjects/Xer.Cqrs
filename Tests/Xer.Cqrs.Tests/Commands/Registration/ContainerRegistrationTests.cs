using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
{
    public class ContainerRegistrationTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Resolve_All_Command_Handlers()
            {
                var container = new Container();
                container.Register<ICommandHandler<DoSomethingAsyncCommand>>(() => new TestCommandHandler(_testOutputHelper), Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerCommandHandlerResolver(containerAdapter);

                CommandHandlerDelegate commandHandlerDelegate = resolver.ResolveCommandHandler<DoSomethingAsyncCommand>();
                
                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new DoSomethingAsyncCommand());

                // Get instance from container to see if it was invoked.
                var registeredCommandHandler = (TestCommandHandler)container.GetInstance<ICommandHandler<DoSomethingAsyncCommand>>();

                Assert.Equal(1, registeredCommandHandler.HandledCommands.Count);
                Assert.Contains(registeredCommandHandler.HandledCommands, c => c is DoSomethingAsyncCommand);
            }
        }

        #endregion Register Method
    }
}
