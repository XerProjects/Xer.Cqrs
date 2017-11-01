using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands
{
    public class CommandHandlerDelegateTests
    {
        public class InvokeMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public InvokeMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Invoke_The_Actual_Registered_Command_Handler()
            {
                var commandHandler = new TestCommandHandler(_testOutputHelper);

                var registration = new CommandHandlerRegistration();
                registration.Register(() => (ICommandHandler<DoSomethingAsyncCommand>)commandHandler);

                CommandHandlerDelegate commandHandlerDelegate = registration.ResolveCommandHandler<DoSomethingAsyncCommand>();

                Assert.NotNull(commandHandlerDelegate);

                // Invoke.
                await commandHandlerDelegate.Invoke(new DoSomethingAsyncCommand());

                // Check if actual command handler instance was invoked.
                Assert.Equal(1, commandHandler.HandledCommands.Count);
                Assert.Contains(commandHandler.HandledCommands, c => c is DoSomethingAsyncCommand);
            }

            [Fact]
            public Task Should_Check_For_Correct_Command_Type()
            {
                return Assert.ThrowsAnyAsync<ArgumentException>(async () =>
                {
                    var commandHandler = new TestCommandHandler(_testOutputHelper);

                    var registration = new CommandHandlerRegistration();
                    registration.Register(() => (ICommandHandler<DoSomethingAsyncCommand>)commandHandler);

                    CommandHandlerDelegate commandHandlerDelegate = registration.ResolveCommandHandler<DoSomethingAsyncCommand>();

                    Assert.NotNull(commandHandlerDelegate);

                    try
                    { 
                        // This delegate handles DoSomethingAsyncCommand, but was passed in a DoSomethingCommand.
                        await commandHandlerDelegate.Invoke(new DoSomethingCommand());
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }
    }
}
