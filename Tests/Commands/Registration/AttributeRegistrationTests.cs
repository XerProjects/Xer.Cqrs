using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
{
    public class AttributeRegistrationTests
    {
        #region RegisterCommandHandlerAttributes Method

        public class RegisterCommandHandlerAttributes
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterCommandHandlerAttributes(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public async Task Should_Register_All_Command_Handlers_Methods()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                Assert.NotNull(commandHandlerDelegate);

                // Delegate should invoke the actual command handler - TestAttributedCommandHandler.
                await commandHandlerDelegate.Invoke(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public async Task Should_Register_All_Command_Handlers_Method_Registrations()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(CommandHandlerAttributeMethod.FromType(() => commandHandler));

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                Assert.NotNull(commandHandlerDelegate);

                // Delegate should invoke the actual command handler - TestAttributedCommandHandler.
                await commandHandlerDelegate.Invoke(new TestCommand());

                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is TestCommand);
            }

            [Fact]
            public void Should_Not_Allow_Sync_Methods_With_Cancellation_Token()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new SingleMessageHandlerRegistration();
                        registration.RegisterCommandHandlerAttributes(() => new TestAttributedSyncCommandHandlerWithCancellationToken(_outputHelper));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion RegisterCommandHandlerAttributes Method
    }
}
