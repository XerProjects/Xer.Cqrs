using Xer.Cqrs.CommandStack;
using Xer.Cqrs.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
{
    public class BasicRegistrationTests
    {
        #region RegisterCommandHandlerMethod Method

        public class RegisterCommandHandlerMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterCommandHandlerMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Should_Register_All_Command_Handlers()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => commandHandler.AsCommandSyncHandler<TestCommand>());

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                Assert.NotNull(commandHandlerDelegate);

                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }
        }

        #endregion RegisterCommandHandlerMethod Method
    }
}
