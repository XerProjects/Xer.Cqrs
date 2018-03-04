using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.CommandStack.Tests.Registration
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
            public async Task ShouldRegisterAllMethodsOfTypeThatIsMarkedWithCommandHandlerAttribute()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(() => commandHandler);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                commandHandlerDelegate.Should().NotBeNull();

                // Delegate should invoke the actual command handler - TestAttributedCommandHandler.
                await commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }

            [Fact]
            public async Task ShouldRegisterAllCommandHandlerAttributeMethodObjects()
            {
                var commandHandler = new TestAttributedCommandHandler(_outputHelper);

                // Get methods marked with [CommandHandler] attribute.
                IEnumerable<CommandHandlerAttributeMethod> methods = CommandHandlerAttributeMethod.FromType(() => commandHandler);

                var registration = new SingleMessageHandlerRegistration();
                registration.RegisterCommandHandlerAttributes(methods);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate commandHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestCommand));

                commandHandlerDelegate.Should().NotBeNull();

                // Delegate should invoke the actual command handler - TestAttributedCommandHandler.
                await commandHandlerDelegate.Invoke(new TestCommand());

                commandHandler.HandledCommands.Should().HaveCount(1);
                commandHandler.HasHandledCommand<TestCommand>().Should().BeTrue();
            }

            [Fact]
            public void ShouldNotAllowSyncMethodsWithCancellationToken()
            {
                Action action = () =>
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
                };

                action.Should().Throw<InvalidOperationException>();
            }
        }

        #endregion RegisterCommandHandlerAttributes Method
    }
}
