using FluentAssertions;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registration;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.CommandStack.Tests.Registration
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
            public void ShouldRegisterCommandHandler()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => commandHandler.AsCommandSyncHandler<TestCommand>());

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                commandHandlerDelegate.Should().NotBeNull();

                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }

            [Fact]
            public void ShouldRegisterCommandAsyncHandler()
            {
                var commandHandler = new TestCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandler(() => commandHandler.AsCommandAsyncHandler<TestCommand>());

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                commandHandlerDelegate.Should().NotBeNull();

                // Delegate should invoke the actual command handler - TestCommandHandler.
                commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }
        }

        #endregion RegisterCommandHandlerMethod Method
    }
}
