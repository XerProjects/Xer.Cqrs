using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
{
    public class BasicRegistrationTests
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
            public void Should_Store_All_Command_Handlers()
            {
                var commandHandler = new TestCommandHandler(_testOutputHelper);

                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandHandler<DoSomethingAsyncCommand>)commandHandler);

                CommandHandlerDelegate commandHandlerDelegate = registration.ResolveCommandHandler<DoSomethingAsyncCommand>();

                Assert.NotNull(commandHandlerDelegate);

                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new DoSomethingAsyncCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingAsyncCommand);
            }
        }

        #endregion Register Method
    }
}
